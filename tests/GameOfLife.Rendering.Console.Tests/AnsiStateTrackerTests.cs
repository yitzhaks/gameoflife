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
}
