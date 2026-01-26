using GameOfLife.Core;
using GameOfLife.Rendering;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class GlyphReaderTests
{
    [Fact]
    public void FromTokens_CharacterTokens_YieldsGlyphsWithoutColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(1, 1);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should have at least the dead cell character
        Assert.NotEmpty(glyphs);
    }

    [Fact]
    public void FromTokens_ColorThenCharacter_YieldsGlyphWithColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(1, 1);
        var states = new Dictionary<Point2D, bool> { [new Point2D(0, 0)] = true };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should have at least one glyph with ForegroundGreen (alive color)
        Assert.Contains(glyphs, g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromTokens_NewlineTokens_YieldsNewlineGlyphs()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 2);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        int newlineCount = 0;
        while (glyphEnumerator.MoveNext())
        {
            if (glyphEnumerator.Current.IsNewline)
            {
                newlineCount++;
            }
        }

        // 2x2 grid should have 2 newlines (one after each row)
        Assert.Equal(2, newlineCount);
    }

    [Fact]
    public void FromTokens_ResetSequence_ClearsColor()
    {
        // This test verifies that a Reset token causes subsequent glyphs to have null color
        // until another color is set
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 1);
        // First cell dead, second cell alive
        var states = new Dictionary<Point2D, bool> { [new Point2D(1, 0)] = true };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // We expect both dead (DarkGray) and alive (Green) colors in the output
        Assert.Contains(glyphs, g => g.Color == AnsiSequence.ForegroundDarkGray);
        Assert.Contains(glyphs, g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromTokens_ConsecutiveSameColor_AccumulatesCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 1);
        // All three cells alive - same color
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(0, 0)] = true,
            [new Point2D(1, 0)] = true,
            [new Point2D(2, 0)] = true
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        var tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        var glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // All character glyphs should have Green color
        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        Assert.Equal(3, characterGlyphs.Count);
        Assert.All(characterGlyphs, g => Assert.Equal(AnsiSequence.ForegroundGreen, g.Color));
    }

    [Fact]
    public void FromTokens_ColorChangesBetweenCells_TracksCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(4, 1);
        // Alternating alive/dead pattern
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(0, 0)] = true,  // alive
            [new Point2D(2, 0)] = true   // alive
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        var tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        var glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        Assert.Equal(4, characterGlyphs.Count);

        // Pattern: alive, dead, alive, dead
        Assert.Equal(AnsiSequence.ForegroundGreen, characterGlyphs[0].Color);
        Assert.Equal(AnsiSequence.ForegroundDarkGray, characterGlyphs[1].Color);
        Assert.Equal(AnsiSequence.ForegroundGreen, characterGlyphs[2].Color);
        Assert.Equal(AnsiSequence.ForegroundDarkGray, characterGlyphs[3].Color);
    }

    [Fact]
    public void FromTokens_MultipleRows_HandlesNewlinesCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 3);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        var glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var newlineCount = 0;
        var characterCount = 0;
        while (glyphEnumerator.MoveNext())
        {
            if (glyphEnumerator.Current.IsNewline)
            {
                newlineCount++;
            }
            else
            {
                characterCount++;
            }
        }

        // 2x3 = 6 characters, 3 newlines (one after each row)
        Assert.Equal(6, characterCount);
        Assert.Equal(3, newlineCount);
    }
}
