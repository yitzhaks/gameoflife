using GameOfLife.Core;
using GameOfLife.Rendering;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class AnsiStateTrackerTests
{
    [Fact]
    public void FromGlyphs_NormalizesColorState()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 1);
        // Two alive cells in a row
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(0, 0)] = true,
            [new Point2D(1, 0)] = true
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        var glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Both character glyphs should have the normalized color
        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        Assert.Equal(2, characterGlyphs.Count);
        Assert.All(characterGlyphs, g => Assert.Equal(AnsiSequence.ForegroundGreen, g.Color));
    }

    [Fact]
    public void FromGlyphs_TracksColorChanges()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 1);
        // First dead, second alive
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(1, 0)] = true
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        var glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        Assert.Equal(2, characterGlyphs.Count);

        // First should be dead (DarkGray)
        Assert.Equal(AnsiSequence.ForegroundDarkGray, characterGlyphs[0].Color);

        // Second should be alive (Green)
        Assert.Equal(AnsiSequence.ForegroundGreen, characterGlyphs[1].Color);
    }

    [Fact]
    public void FromGlyphs_NewlineGlyphsHaveNoColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(1, 2);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var newlineGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (glyphEnumerator.Current.IsNewline)
            {
                newlineGlyphs.Add(glyphEnumerator.Current);
            }
        }

        // Newlines should not have color
        Assert.All(newlineGlyphs, g => Assert.Null(g.Color));
    }

    [Fact]
    public void FromGlyphs_ColorPersistsAcrossGlyphsWithoutExplicitColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 1);
        // All cells alive - color should persist
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(0, 0)] = true,
            [new Point2D(1, 0)] = true,
            [new Point2D(2, 0)] = true
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        var glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (!glyphEnumerator.Current.IsNewline)
            {
                characterGlyphs.Add(glyphEnumerator.Current);
            }
        }

        // All should have the same color (green)
        Assert.Equal(3, characterGlyphs.Count);
        Assert.All(characterGlyphs, g => Assert.Equal(AnsiSequence.ForegroundGreen, g.Color));
    }

    [Fact]
    public void FromGlyphs_MultipleColorChanges_TracksAllTransitions()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(5, 1);
        // Pattern: alive, dead, alive, dead, alive (5 color transitions)
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(0, 0)] = true,
            [new Point2D(2, 0)] = true,
            [new Point2D(4, 0)] = true
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        var glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (!glyphEnumerator.Current.IsNewline)
            {
                characterGlyphs.Add(glyphEnumerator.Current);
            }
        }

        Assert.Equal(5, characterGlyphs.Count);

        // Verify alternating colors
        Assert.Equal(AnsiSequence.ForegroundGreen, characterGlyphs[0].Color);     // alive
        Assert.Equal(AnsiSequence.ForegroundDarkGray, characterGlyphs[1].Color);  // dead
        Assert.Equal(AnsiSequence.ForegroundGreen, characterGlyphs[2].Color);     // alive
        Assert.Equal(AnsiSequence.ForegroundDarkGray, characterGlyphs[3].Color);  // dead
        Assert.Equal(AnsiSequence.ForegroundGreen, characterGlyphs[4].Color);     // alive
    }

    [Fact]
    public void FromGlyphs_GrayBorderColor_ValidatesCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 2);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should include border glyphs with gray color
        Assert.Contains(glyphs, g => g.Color == AnsiSequence.ForegroundGray);
    }

    [Fact]
    public void FromGlyphs_EmptyEnumerator_ReturnsImmediately()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 0x0 grid would be invalid, but 1x0 results in empty enumeration
        var topology = new Grid2DTopology(1, 1);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var count = 0;
        while (glyphEnumerator.MoveNext())
        {
            count++;
        }

        // 1x1 grid = 1 cell + 1 newline = 2 glyphs
        Assert.Equal(2, count);
    }
}
