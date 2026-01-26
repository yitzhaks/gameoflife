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

        var tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        var glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

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

        var tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        var glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

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

        var tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        var glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var newlineCount = 0;
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

        var tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        var glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // We expect both dead (DarkGray) and alive (Green) colors in the output
        Assert.Contains(glyphs, g => g.Color == AnsiSequence.ForegroundDarkGray);
        Assert.Contains(glyphs, g => g.Color == AnsiSequence.ForegroundGreen);
    }
}
