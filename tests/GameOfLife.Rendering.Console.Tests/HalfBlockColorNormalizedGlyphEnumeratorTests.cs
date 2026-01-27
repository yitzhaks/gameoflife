using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public sealed class HalfBlockColorNormalizedGlyphEnumeratorTests : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    public void Dispose()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    private IGeneration<Point2D, bool> CreateGeneration(Size2D size, params Point2D[] aliveCells)
    {
        using var builder = new RectangularGenerationBuilder(size);
        foreach (Point2D cell in aliveCells)
        {
            builder[cell] = true;
        }

        IGeneration<Point2D, bool> generation = builder.Build();
        if (generation is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return generation;
    }

    private static HalfBlockColorNormalizedGlyphEnumerator CreateEnumerator(
        RectangularTopology topology,
        IGeneration<Point2D, bool> generation,
        bool showBorder = false)
    {
        var layout = new HalfBlockLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: showBorder);
        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        HalfBlockGlyphEnumerator glyphEnumerator = GlyphReader.FromTokensHalfBlock(tokenEnumerator);
        return AnsiStateTracker.FromGlyphsHalfBlock(glyphEnumerator);
    }

    // Color normalization tests

    [Fact]
    public void MoveNext_GlyphsWithoutExplicitColor_InheritCurrentState()
    {
        var topology = new RectangularTopology((2, 2));
        // All alive - color set once, should persist to all glyphs
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0), (0, 1), (1, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.IsNewline)
            {
                characterGlyphs.Add(enumerator.Current);
            }
        }

        // All character glyphs should have the same normalized color
        characterGlyphs.Count.ShouldBe(2); // 2x2 grid packed = 2 columns, 1 row
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void MoveNext_ColorNormalization_AllGlyphsHaveColor()
    {
        var topology = new RectangularTopology((4, 2));
        // Alternating pattern to force color changes
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 2), (0, 0), (0, 1), (2, 0), (2, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.IsNewline)
            {
                characterGlyphs.Add(enumerator.Current);
            }
        }

        // Every character glyph should have a color (no nulls after normalization)
        characterGlyphs.ShouldAllBe(g => g.Color.HasValue);
    }

    // Foreground color state tracking tests

    [Fact]
    public void MoveNext_ForegroundColorStateTracking_TracksChanges()
    {
        var topology = new RectangularTopology((4, 2));
        // Alive, dead, alive, dead pattern
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 2), (0, 0), (0, 1), (2, 0), (2, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.IsNewline)
            {
                characterGlyphs.Add(enumerator.Current);
            }
        }

        characterGlyphs.Count.ShouldBe(4);

        // First cell (0,0)+(0,1) both alive -> green
        characterGlyphs[0].Color.ShouldBe(AnsiSequence.ForegroundGreen);

        // Second cell (1,0)+(1,1) both dead -> dark gray
        characterGlyphs[1].Color.ShouldBe(AnsiSequence.ForegroundDarkGray);

        // Third cell (2,0)+(2,1) both alive -> green
        characterGlyphs[2].Color.ShouldBe(AnsiSequence.ForegroundGreen);

        // Fourth cell (3,0)+(3,1) both dead -> dark gray
        characterGlyphs[3].Color.ShouldBe(AnsiSequence.ForegroundDarkGray);
    }

    [Fact]
    public void MoveNext_ForegroundColorPersists_AcrossConsecutiveSameColorGlyphs()
    {
        var topology = new RectangularTopology((4, 2));
        // All alive - same color throughout
        IGeneration<Point2D, bool> generation = CreateGeneration(
            (4, 2),
            (0, 0), (0, 1), (1, 0), (1, 1), (2, 0), (2, 1), (3, 0), (3, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.IsNewline)
            {
                characterGlyphs.Add(enumerator.Current);
            }
        }

        // All should be green
        characterGlyphs.Count.ShouldBe(4);
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    // Background color state tracking tests

    [Fact]
    public void MoveNext_BackgroundColorStateTracking_TracksChanges()
    {
        var topology = new RectangularTopology((4, 2));
        // Pattern: both alive, both dead, both alive, both dead
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 2), (0, 0), (0, 1), (2, 0), (2, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.IsNewline)
            {
                characterGlyphs.Add(enumerator.Current);
            }
        }

        characterGlyphs.Count.ShouldBe(4);

        // The normalizer tracks background color state.
        // BackgroundDefault is now preserved through the pipeline so it can be emitted downstream.
        characterGlyphs[0].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDefault); // Both alive -> full block -> BackgroundDefault
        characterGlyphs[1].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDarkGray); // Both dead -> explicit DarkGray
        characterGlyphs[2].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDefault); // Both alive -> full block -> BackgroundDefault
        characterGlyphs[3].BackgroundColor.ShouldBe(AnsiSequence.BackgroundDarkGray); // Both dead -> explicit DarkGray
    }

    [Fact]
    public void MoveNext_BackgroundColorPersists_WhenUnchanged()
    {
        var topology = new RectangularTopology((4, 2));
        // All dead - dark gray background throughout
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 2));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.IsNewline)
            {
                characterGlyphs.Add(enumerator.Current);
            }
        }

        // All should have dark gray background
        characterGlyphs.Count.ShouldBe(4);
        characterGlyphs.ShouldAllBe(g => g.BackgroundColor == AnsiSequence.BackgroundDarkGray);
    }

    [Fact]
    public void MoveNext_TopAliveBottomDead_UsesCorrectBackground()
    {
        var topology = new RectangularTopology((2, 2));
        // Top row alive, bottom row dead
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.IsNewline)
            {
                characterGlyphs.Add(enumerator.Current);
            }
        }

        // Upper half block should have dark gray background
        characterGlyphs.ShouldAllBe(g => g.BackgroundColor == AnsiSequence.BackgroundDarkGray);
    }

    // Newline passthrough tests

    [Fact]
    public void MoveNext_NewlinePassthrough_DoesNotAffectColorState()
    {
        var topology = new RectangularTopology((2, 4));
        // First row alive, second row dead
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 4), (0, 0), (1, 0), (0, 1), (1, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        // Should have newlines between rows
        glyphs.ShouldContain(g => g.IsNewline);

        // Newlines should have no color
        var newlines = glyphs.Where(g => g.IsNewline).ToList();
        newlines.ShouldAllBe(g => g.Color == null && g.BackgroundColor == null);

        // Character glyphs should still have colors
        var characters = glyphs.Where(g => !g.IsNewline).ToList();
        characters.ShouldAllBe(g => g.Color.HasValue);
    }

    [Fact]
    public void MoveNext_NewlinePassthrough_ColorStatePreservedAcrossNewlines()
    {
        var topology = new RectangularTopology((2, 4));
        // All cells alive to test color persistence across newlines
        IGeneration<Point2D, bool> generation = CreateGeneration(
            (2, 4),
            (0, 0), (1, 0), (0, 1), (1, 1),
            (0, 2), (1, 2), (0, 3), (1, 3));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var characterGlyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            if (!enumerator.Current.IsNewline)
            {
                characterGlyphs.Add(enumerator.Current);
            }
        }

        // All should maintain green color across row boundaries
        characterGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    // Empty enumeration tests

    [Fact]
    public void MoveNext_EmptyEnumeration_ReturnsFalse()
    {
        // Create a minimal grid (2x2 is the minimum for half-block)
        var topology = new RectangularTopology((1, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((1, 2));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        // Should return true for the first glyph (1 cell + newline)
        enumerator.MoveNext().ShouldBeTrue();
        enumerator.MoveNext().ShouldBeTrue(); // newline

        // After exhausting all glyphs, should return false
        enumerator.MoveNext().ShouldBeFalse();
    }

    [Fact]
    public void MoveNext_AfterExhaustion_ContinuesToReturnFalse()
    {
        var topology = new RectangularTopology((1, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((1, 2));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        // Exhaust the enumerator
        while (enumerator.MoveNext())
        {
        }

        // Multiple calls should continue to return false
        enumerator.MoveNext().ShouldBeFalse();
        enumerator.MoveNext().ShouldBeFalse();
        enumerator.MoveNext().ShouldBeFalse();
    }

    // GetEnumerator tests

    [Fact]
    public void GetEnumerator_ReturnsSelf()
    {
        var topology = new RectangularTopology((2, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        // GetEnumerator should return the same enumerator (for foreach support)
        HalfBlockColorNormalizedGlyphEnumerator result = enumerator.GetEnumerator();

        // Should be able to iterate with the returned enumerator
        result.MoveNext().ShouldBeTrue();
    }

    [Fact]
    public void GetEnumerator_SupportsForEachPattern()
    {
        var topology = new RectangularTopology((2, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0), (0, 1), (1, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        var glyphs = new List<Glyph>();

        // Use foreach pattern manually since ref struct can't be used with LINQ
        foreach (Glyph glyph in enumerator)
        {
            glyphs.Add(glyph);
        }

        glyphs.ShouldNotBeEmpty();
    }

    // Validation of unknown sequences tests

    [Fact]
    public void ValidateSequence_UnknownForegroundSequence_ThrowsInvalidOperationException()
    {
        // This test verifies that validation throws for unknown sequences
        // The ValidateSequence method is internal and shared with ColorNormalizedGlyphEnumerator
        var invalidSequence = (AnsiSequence)254;

        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
            ColorNormalizedGlyphEnumerator.ValidateSequence(invalidSequence));

        exception.Message.ShouldContain("Unknown ANSI sequence");
        exception.Message.ShouldContain("254");
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
        // Should not throw for any known sequence
        ColorNormalizedGlyphEnumerator.ValidateSequence(sequence);

    // Integration tests with borders

    [Fact]
    public void MoveNext_WithBorder_NormalizesBorderColors()
    {
        var topology = new RectangularTopology((2, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation, showBorder: true);

        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        // Should contain border glyphs with gray color
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGray);
    }

    [Fact]
    public void MoveNext_WithBorder_HandlesColorTransitionsCorrectly()
    {
        var topology = new RectangularTopology((2, 2));
        // All alive
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0), (0, 1), (1, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation, showBorder: true);

        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        // Should have both gray (border) and green (cells) colors
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGray);
        glyphs.ShouldContain(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    // Current property tests

    [Fact]
    public void Current_BeforeMoveNext_ReturnsDefault()
    {
        var topology = new RectangularTopology((2, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        // Current before MoveNext should be default
        Glyph current = enumerator.Current;
        current.ShouldBe(default);
    }

    [Fact]
    public void Current_AfterMoveNext_ReturnsNormalizedGlyph()
    {
        var topology = new RectangularTopology((2, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0), (0, 1), (1, 1));

        HalfBlockColorNormalizedGlyphEnumerator enumerator = CreateEnumerator(topology, generation);

        enumerator.MoveNext().ShouldBeTrue();

        Glyph current = enumerator.Current;
        current.Color.ShouldBe(AnsiSequence.ForegroundGreen);
    }

    // Factory method tests

    [Fact]
    public void FromGlyphsHalfBlock_CreatesWorkingEnumerator()
    {
        var topology = new RectangularTopology((2, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0), (0, 1), (1, 1));

        var layout = new HalfBlockLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);
        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        HalfBlockGlyphEnumerator glyphEnumerator = GlyphReader.FromTokensHalfBlock(tokenEnumerator);

        // Use factory method
        HalfBlockColorNormalizedGlyphEnumerator enumerator = AnsiStateTracker.FromGlyphsHalfBlock(glyphEnumerator);

        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        glyphs.ShouldNotBeEmpty();
        glyphs.Where(g => !g.IsNewline).ShouldAllBe(g => g.Color.HasValue);
    }
}
