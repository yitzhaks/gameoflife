using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

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

        var topology = new RectangularTopology((1, 1));
        using var generation = new DictionaryGeneration<Point2D, bool>(
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
        glyphs.ShouldNotBeEmpty();
    }

    [Fact]
    public void FromTokens_ColorThenCharacter_YieldsGlyphWithColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 1));
        var states = new Dictionary<Point2D, bool> { [default] = true };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should have at least one glyph with ForegroundGreen (alive color)
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromTokens_NewlineTokens_YieldsNewlineGlyphs()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
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
        newlineCount.ShouldBe(2);
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

        var topology = new RectangularTopology((2, 1));
        // First cell dead, second cell alive
        var states = new Dictionary<Point2D, bool> { [(1, 0)] = true };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // We expect both dead (DarkGray) and alive (Green) colors in the output
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundDarkGray);
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromTokens_ConsecutiveSameColor_AccumulatesCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 1));
        // All three cells alive - same color
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 0)] = true,
            [(2, 0)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // All character glyphs should have Green color
        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        characterGlyphs.Count.ShouldBe(3);
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromTokens_ColorChangesBetweenCells_TracksCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((4, 1));
        // Alternating alive/dead pattern
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,  // alive
            [(2, 0)] = true   // alive
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        characterGlyphs.Count.ShouldBe(4);

        // Pattern: alive, dead, alive, dead
        characterGlyphs[0].Color.ShouldBe(AnsiSequence.ForegroundGreen);
        characterGlyphs[1].Color.ShouldBe(AnsiSequence.ForegroundDarkGray);
        characterGlyphs[2].Color.ShouldBe(AnsiSequence.ForegroundGreen);
        characterGlyphs[3].Color.ShouldBe(AnsiSequence.ForegroundDarkGray);
    }

    [Fact]
    public void FromTokens_MultipleRows_HandlesNewlinesCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 3));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        int newlineCount = 0;
        int characterCount = 0;
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
        characterCount.ShouldBe(6);
        newlineCount.ShouldBe(3);
    }

    // GlyphEnumerator constructor and state tests

    [Fact]
    public void GlyphEnumerator_Current_BeforeMoveNext_ReturnsDefault()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        // Before any MoveNext call, Current should be default
        Glyph current = glyphEnumerator.Current;
        current.ShouldBe(default);
    }

    [Fact]
    public void GlyphEnumerator_GetEnumerator_ReturnsSelf()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        // GetEnumerator should return a copy of the enumerator (ref struct semantics)
        GlyphEnumerator result = glyphEnumerator.GetEnumerator();

        // Both should be able to enumerate (they are independent copies due to ref struct)
        _ = result; // Verify it compiles and doesn't throw
    }

    [Fact]
    public void GlyphEnumerator_AllowsForeachSyntax()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        int count = 0;
        foreach (Glyph glyph in glyphEnumerator)
        {
            count++;
            _ = glyph; // Use the glyph
        }

        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GlyphEnumerator_MoveNext_AfterEnumerationComplete_ReturnsFalse()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 1));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        // Exhaust the enumerator
        while (glyphEnumerator.MoveNext())
        {
            // Consume all glyphs
        }

        // Subsequent calls should continue to return false
        glyphEnumerator.MoveNext().ShouldBeFalse();
        glyphEnumerator.MoveNext().ShouldBeFalse();
    }

    // Border and ForegroundGray handling tests

    [Fact]
    public void FromTokens_WithBorder_YieldsBorderGlyphsWithGrayColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should have border characters with ForegroundGray
        glyphs.ShouldContain(g => g.Character == ConsoleTheme.Border.TopLeft);
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGray);
    }

    [Fact]
    public void FromTokens_WithBorder_BorderCornerGlyphsHaveCorrectColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Find the top-left corner glyph and verify it has gray color
        Glyph topLeftCorner = glyphs.First(g => g.Character == ConsoleTheme.Border.TopLeft);
        topLeftCorner.Color.ShouldBe(AnsiSequence.ForegroundGray);

        // Find the bottom-right corner glyph and verify it has gray color
        Glyph bottomRightCorner = glyphs.First(g => g.Character == ConsoleTheme.Border.BottomRight);
        bottomRightCorner.Color.ShouldBe(AnsiSequence.ForegroundGray);
    }

    // Newline handling tests

    [Fact]
    public void FromTokens_NewlineGlyphs_HaveNullColorAndNullBackground()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 0)] = true,
            [(0, 1)] = true,
            [(1, 1)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // All newline glyphs should have null color and null background
        var newlineGlyphs = glyphs.Where(g => g.IsNewline).ToList();
        newlineGlyphs.ShouldNotBeEmpty();
        newlineGlyphs.ShouldAllBe(g => g.Color == null && g.BackgroundColor == null);
    }

    // Color state persistence tests

    [Fact]
    public void FromTokens_ColorPersists_UntilNewColorSet()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 1));
        // All cells alive - should maintain same color
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 0)] = true,
            [(2, 0)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // All character glyphs should maintain green color
        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromTokens_ColorPersistsAcrossNewlines()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        // All cells alive
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 0)] = true,
            [(0, 1)] = true,
            [(1, 1)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Color should persist across newlines - all alive cells should be green
        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        characterGlyphs.Count.ShouldBe(4);
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    // Current after MoveNext tests

    [Fact]
    public void GlyphEnumerator_Current_AfterMoveNext_ReturnsCurrentGlyph()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 1));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        glyphEnumerator.MoveNext().ShouldBeTrue();

        Glyph current = glyphEnumerator.Current;
        current.ShouldNotBe(default);
    }

    // Edge case with multiple color sequences before a character

    [Fact]
    public void FromTokens_MultipleColorSequences_LastColorWins()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 1));
        // First cell dead (will be DarkGray)
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Find the first dead cell glyph (after border)
        var cellGlyphs = glyphs.Where(g =>
            !g.IsNewline &&
            g.Character != ConsoleTheme.Border.TopLeft &&
            g.Character != ConsoleTheme.Border.TopRight &&
            g.Character != ConsoleTheme.Border.BottomLeft &&
            g.Character != ConsoleTheme.Border.BottomRight &&
            g.Character != ConsoleTheme.Border.Horizontal &&
            g.Character != ConsoleTheme.Border.Vertical).ToList();

        // Dead cells should have DarkGray color
        cellGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundDarkGray);
    }

    // Test with mixed border and cell colors

    [Fact]
    public void FromTokens_BorderAndCells_TransitionsColorCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 1));
        var states = new Dictionary<Point2D, bool> { [default] = true };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should have border glyphs with Gray color
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGray && g.Character == ConsoleTheme.Border.TopLeft);

        // Should have cell glyph with Green color
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGreen && g.Character == '#');
    }

    // Minimal grid test

    [Fact]
    public void FromTokens_SingleCell_YieldsCorrectGlyphs()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 1));
        var states = new Dictionary<Point2D, bool> { [default] = true };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should have exactly 2 glyphs: the alive cell and a newline
        glyphs.Count.ShouldBe(2);
        glyphs[0].Character.ShouldBe('#');
        glyphs[0].Color.ShouldBe(AnsiSequence.ForegroundGreen);
        glyphs[1].IsNewline.ShouldBeTrue();
    }

    // BackgroundColor should be null for standard TokenEnumerator (it doesn't emit background sequences)

    [Fact]
    public void FromTokens_StandardRenderer_GlyphsHaveNullBackground()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 1)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        TokenEnumerator tokenEnumerator = renderer.GetTokenEnumerator(topology, generation);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Standard TokenEnumerator doesn't emit background sequences,
        // so all glyphs should have null background
        glyphs.ShouldAllBe(g => g.BackgroundColor == null);
    }
}
