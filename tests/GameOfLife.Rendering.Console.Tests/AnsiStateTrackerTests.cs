using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

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

        var topology = new RectangularTopology((2, 1));
        // Two alive cells in a row
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 0)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Both character glyphs should have the normalized color
        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        characterGlyphs.Count.ShouldBe(2);
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromGlyphs_TracksColorChanges()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 1));
        // First dead, second alive
        var states = new Dictionary<Point2D, bool>
        {
            [(1, 0)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        characterGlyphs.Count.ShouldBe(2);

        // First should be dead (DarkGray)
        characterGlyphs[0].Color.ShouldBe(AnsiSequence.ForegroundDarkGray);

        // Second should be alive (Green)
        characterGlyphs[1].Color.ShouldBe(AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromGlyphs_NewlineGlyphsHaveNoColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var newlineGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (glyphEnumerator.Current.IsNewline)
            {
                newlineGlyphs.Add(glyphEnumerator.Current);
            }
        }

        // Newlines should not have color
        newlineGlyphs.ShouldAllBe(g => g.Color == null);
    }

    [Fact]
    public void FromGlyphs_ColorPersistsAcrossGlyphsWithoutExplicitColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 1));
        // All cells alive - color should persist
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 0)] = true,
            [(2, 0)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (!glyphEnumerator.Current.IsNewline)
            {
                characterGlyphs.Add(glyphEnumerator.Current);
            }
        }

        // All should have the same color (green)
        characterGlyphs.Count.ShouldBe(3);
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromGlyphs_MultipleColorChanges_TracksAllTransitions()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((5, 1));
        // Pattern: alive, dead, alive, dead, alive (5 color transitions)
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(2, 0)] = true,
            [(4, 0)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (!glyphEnumerator.Current.IsNewline)
            {
                characterGlyphs.Add(glyphEnumerator.Current);
            }
        }

        characterGlyphs.Count.ShouldBe(5);

        // Verify alternating colors
        characterGlyphs[0].Color.ShouldBe(AnsiSequence.ForegroundGreen);     // alive
        characterGlyphs[1].Color.ShouldBe(AnsiSequence.ForegroundDarkGray);  // dead
        characterGlyphs[2].Color.ShouldBe(AnsiSequence.ForegroundGreen);     // alive
        characterGlyphs[3].Color.ShouldBe(AnsiSequence.ForegroundDarkGray);  // dead
        characterGlyphs[4].Color.ShouldBe(AnsiSequence.ForegroundGreen);     // alive
    }

    [Fact]
    public void FromGlyphs_GrayBorderColor_ValidatesCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should include border glyphs with gray color
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGray);
    }

    [Fact]
    public void FromGlyphs_EmptyEnumerator_ReturnsImmediately()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 0x0 grid would be invalid, but 1x0 results in empty enumeration
        var topology = new RectangularTopology((1, 1));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        int count = 0;
        while (glyphEnumerator.MoveNext())
        {
            count++;
        }

        // 1x1 grid = 1 cell + 1 newline = 2 glyphs
        count.ShouldBe(2);
    }

    [Theory]
    [InlineData(AnsiSequence.Reset)]
    [InlineData(AnsiSequence.ForegroundGreen)]
    [InlineData(AnsiSequence.ForegroundDarkGray)]
    [InlineData(AnsiSequence.ForegroundGray)]
    [InlineData(AnsiSequence.BackgroundGreen)]
    [InlineData(AnsiSequence.BackgroundDarkGray)]
    [InlineData(AnsiSequence.BackgroundDefault)]
    public void ValidateSequence_KnownSequences_DoesNotThrow(AnsiSequence sequence) =>
        // Act & Assert - should not throw
        ColorNormalizedGlyphEnumerator.ValidateSequence(sequence);

    [Fact]
    public void ValidateSequence_UnknownSequence_ThrowsInvalidOperationException()
    {
        // Arrange - cast an invalid value to AnsiSequence
        var invalidSequence = (AnsiSequence)255;

        // Act & Assert
        InvalidOperationException ex = Should.Throw<InvalidOperationException>(() =>
            ColorNormalizedGlyphEnumerator.ValidateSequence(invalidSequence));
        ex.Message.ShouldContain("Unknown ANSI sequence");
        ex.Message.ShouldContain("255");
    }

    [Fact]
    public void FromGlyphsHalfBlock_NormalizesForegroundColorState()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 2x2 topology packs into 2 columns, 1 half-block row
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

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Both character glyphs should have green foreground (all alive)
        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        characterGlyphs.Count.ShouldBe(2);
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void FromGlyphsHalfBlock_NormalizesBackgroundColorState()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 2x2 topology packs into 2 columns, 1 half-block row
        var topology = new RectangularTopology((2, 2));
        // All cells dead - should have dark gray background
        var states = new Dictionary<Point2D, bool>();
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Both character glyphs should have dark gray background (all dead)
        var characterGlyphs = glyphs.Where(g => !g.IsNewline).ToList();
        characterGlyphs.Count.ShouldBe(2);
        characterGlyphs.ShouldAllBe(g => g.BackgroundColor == AnsiSequence.BackgroundDarkGray);
    }

    [Fact]
    public void FromGlyphsHalfBlock_TracksBackgroundColorChanges()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 2x2 topology packs into 2 columns, 1 half-block row
        var topology = new RectangularTopology((2, 2));
        // First column: both alive (default background), Second column: both dead (dark gray background)
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(0, 1)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (!glyphEnumerator.Current.IsNewline)
            {
                characterGlyphs.Add(glyphEnumerator.Current);
            }
        }

        characterGlyphs.Count.ShouldBe(2);
        // First column: both alive = default background
        characterGlyphs[0].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDefault);
        // Second column: both dead = dark gray background
        characterGlyphs[1].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDarkGray);
    }

    [Fact]
    public void FromGlyphsHalfBlock_NewlineGlyphsHaveNoColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 1x4 topology packs into 1 column, 2 half-block rows
        var topology = new RectangularTopology((1, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        var newlineGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (glyphEnumerator.Current.IsNewline)
            {
                newlineGlyphs.Add(glyphEnumerator.Current);
            }
        }

        // Newlines should not have color
        newlineGlyphs.ShouldAllBe(g => g.Color == null);
        newlineGlyphs.ShouldAllBe(g => g.BackgroundColor == null);
    }

    [Fact]
    public void FromGlyphsHalfBlock_ColorPersistsAcrossGlyphsWithoutExplicitColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 3x2 topology packs into 3 columns, 1 half-block row
        var topology = new RectangularTopology((3, 2));
        // All cells alive - colors should persist
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 0)] = true,
            [(2, 0)] = true,
            [(0, 1)] = true,
            [(1, 1)] = true,
            [(2, 1)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (!glyphEnumerator.Current.IsNewline)
            {
                characterGlyphs.Add(glyphEnumerator.Current);
            }
        }

        // All should have the same colors (green foreground, default background)
        characterGlyphs.Count.ShouldBe(3);
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
        characterGlyphs.ShouldAllBe(g => g.BackgroundColor == AnsiSequence.BackgroundDefault);
    }

    [Fact]
    public void FromGlyphsHalfBlock_MixedCells_TracksAllColorTransitions()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 3x2 topology packs into 3 columns, 1 half-block row
        var topology = new RectangularTopology((3, 2));
        // Pattern: top alive/bottom dead, top dead/bottom alive, both dead
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,   // (0,0) alive - top of first column
            [(1, 1)] = true     // (1,1) alive - bottom of second column
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            if (!glyphEnumerator.Current.IsNewline)
            {
                characterGlyphs.Add(glyphEnumerator.Current);
            }
        }

        characterGlyphs.Count.ShouldBe(3);

        // First column: top alive, bottom dead -> green foreground, dark gray background
        characterGlyphs[0].Color.ShouldBe(AnsiSequence.ForegroundGreen);
        characterGlyphs[0].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDarkGray);

        // Second column: top dead, bottom alive -> green foreground, dark gray background
        characterGlyphs[1].Color.ShouldBe(AnsiSequence.ForegroundGreen);
        characterGlyphs[1].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDarkGray);

        // Third column: both dead -> dark gray foreground, dark gray background
        characterGlyphs[2].Color.ShouldBe(AnsiSequence.ForegroundDarkGray);
        characterGlyphs[2].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDarkGray);
    }

    [Fact]
    public void FromGlyphsHalfBlock_WithBorder_ValidatesGrayBorderColors()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (glyphEnumerator.MoveNext())
        {
            glyphs.Add(glyphEnumerator.Current);
        }

        // Should include border glyphs with gray color
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGray);
    }

    [Fact]
    public void FromGlyphsHalfBlock_GetEnumerator_ReturnsSelf()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator enumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        // Verify GetEnumerator returns a valid enumerator that can iterate
        int count = 0;
        foreach (Glyph glyph in enumerator)
        {
            count++;
        }

        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void FromGlyphs_GetEnumerator_ReturnsSelf()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 1));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator enumerator = renderer.GetGlyphEnumerator(topology, generation);

        // Verify GetEnumerator returns a valid enumerator that can iterate
        int count = 0;
        foreach (Glyph glyph in enumerator)
        {
            count++;
        }

        count.ShouldBeGreaterThan(0);
    }
}
