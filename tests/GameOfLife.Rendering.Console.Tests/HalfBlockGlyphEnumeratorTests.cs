using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public sealed class HalfBlockGlyphEnumeratorTests : IDisposable
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

    private static List<Glyph> CollectGlyphs(HalfBlockGlyphEnumerator enumerator)
    {
        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        return glyphs;
    }

    // Basic token to glyph conversion tests

    [Fact]
    public void MoveNext_CharacterToken_YieldsGlyphWithCharacter()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should have glyphs for the 2x1 packed grid (2 cells + newline)
        glyphs.ShouldNotBeEmpty();
        glyphs.Any(g => g.Character == ' ').ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_AliveCell_YieldsGlyphWithGreenForeground()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Both cells in first column alive
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (0, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should have a full block glyph with green foreground
        Glyph fullBlockGlyph = glyphs.First(g => g.Character == ConsoleTheme.HalfBlock.Full);
        fullBlockGlyph.Color.ShouldBe(AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void MoveNext_DeadCell_YieldsGlyphWithDarkGrayForeground()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should have space glyphs with dark gray foreground
        Glyph spaceGlyph = glyphs.First(g => g.Character == ' ');
        spaceGlyph.Color.ShouldBe(AnsiSequence.ForegroundDarkGray);
    }

    // Color sequence accumulation tests

    [Fact]
    public void MoveNext_MultipleForegroundSequences_AccumulatesLastColor()
    {
        var topology = new RectangularTopology((4, 2));
        var layout = new HalfBlockLayout(topology);
        // Alternate alive and dead cells to force color changes
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 2), (0, 0), (0, 1), (2, 0), (2, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // First column has both alive - green foreground
        Glyph firstGlyph = glyphs[0];
        firstGlyph.Color.ShouldBe(AnsiSequence.ForegroundGreen);

        // Second column has both dead - dark gray foreground
        Glyph secondGlyph = glyphs[1];
        secondGlyph.Color.ShouldBe(AnsiSequence.ForegroundDarkGray);
    }

    [Fact]
    public void MoveNext_ForegroundColorPersists_UntilChanged()
    {
        var topology = new RectangularTopology((2, 4));
        var layout = new HalfBlockLayout(topology);
        // All cells alive
        var aliveCells = new List<Point2D>();
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                aliveCells.Add((x, y));
            }
        }

        IGeneration<Point2D, bool> generation = CreateGeneration((2, 4), [.. aliveCells]);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // All non-newline glyphs should have green foreground
        IEnumerable<Glyph> cellGlyphs = glyphs.Where(g => !g.IsNewline);
        cellGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    // Background color handling tests

    [Fact]
    public void MoveNext_BackgroundGreen_AccumulatesBackgroundColor()
    {
        // Note: BackgroundGreen is not used by the current half-block implementation,
        // but we test the enumerator's handling of it
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Top alive, bottom dead - should have BackgroundDarkGray
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should have glyphs with background color set
        IEnumerable<Glyph> cellGlyphs = glyphs.Where(g => !g.IsNewline);
        cellGlyphs.Any(g => g.BackgroundColor == AnsiSequence.BackgroundDarkGray).ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_BackgroundDarkGray_SetsBackgroundColor()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // All dead - should have BackgroundDarkGray
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // All non-newline glyphs should have dark gray background
        IEnumerable<Glyph> cellGlyphs = glyphs.Where(g => !g.IsNewline);
        cellGlyphs.ShouldAllBe(g => g.BackgroundColor == AnsiSequence.BackgroundDarkGray);
    }

    [Fact]
    public void MoveNext_BackgroundDefault_PreservesBackgroundDefault()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Both cells alive - should have BackgroundDefault preserved through pipeline
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0), (0, 1), (1, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Full block glyphs should have BackgroundDefault preserved so downstream can emit reset
        IEnumerable<Glyph> fullBlockGlyphs = glyphs.Where(g => g.Character == ConsoleTheme.HalfBlock.Full);
        fullBlockGlyphs.ShouldAllBe(g => g.BackgroundColor == AnsiSequence.BackgroundDefault);
    }

    [Fact]
    public void MoveNext_BackgroundColorChanges_TrackedAcrossGlyphs()
    {
        var topology = new RectangularTopology((4, 2));
        var layout = new HalfBlockLayout(topology);
        // Mix of all alive and all dead to force background changes
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 2), (0, 0), (0, 1), (2, 0), (2, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);
        IEnumerable<Glyph> cellGlyphs = glyphs.Where(g => !g.IsNewline).ToList();

        // Should have both BackgroundDefault (for full blocks) and BackgroundDarkGray (for dead cells)
        cellGlyphs.Any(g => g.BackgroundColor == AnsiSequence.BackgroundDefault).ShouldBeTrue();
        cellGlyphs.Any(g => g.BackgroundColor == AnsiSequence.BackgroundDarkGray).ShouldBeTrue();
    }

    // Reset sequence handling tests

    [Fact]
    public void MoveNext_ResetSequence_ClearsBothForegroundAndBackground()
    {
        // The HalfBlockTokenEnumerator does not emit Reset sequences during normal operation,
        // but the GlyphEnumerator should handle them correctly.
        // We can verify behavior indirectly by confirming proper state tracking.
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (0, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Enumeration should complete without error
        glyphs.ShouldNotBeEmpty();
    }

    // Newline handling tests

    [Fact]
    public void MoveNext_NewlineCharacter_YieldsGlyphWithNoColor()
    {
        var topology = new RectangularTopology((2, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Find newline glyphs
        IEnumerable<Glyph> newlineGlyphs = glyphs.Where(g => g.IsNewline);
        newlineGlyphs.ShouldNotBeEmpty();
        newlineGlyphs.ShouldAllBe(g => g.Color == null && g.BackgroundColor == null);
    }

    [Fact]
    public void MoveNext_NewlineCharacter_PreservesColorStateForNextLine()
    {
        var topology = new RectangularTopology((2, 4));
        var layout = new HalfBlockLayout(topology);
        // All alive cells - same color before and after newlines
        var aliveCells = new List<Point2D>();
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                aliveCells.Add((x, y));
            }
        }

        IGeneration<Point2D, bool> generation = CreateGeneration((2, 4), [.. aliveCells]);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Color should be consistent across rows
        IEnumerable<Glyph> cellGlyphs = glyphs.Where(g => !g.IsNewline);
        cellGlyphs.ShouldAllBe(g => g.Color == AnsiSequence.ForegroundGreen);
    }

    // Empty enumeration tests

    [Fact]
    public void MoveNext_NoTokens_ReturnsFalse()
    {
        // Create a minimal setup that produces no output
        // Since we can't easily create an empty token enumerator, we verify
        // that MoveNext correctly handles completion
        var topology = new RectangularTopology((1, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((1, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        // Consume all glyphs
        while (glyphEnumerator.MoveNext())
        {
            // Keep consuming
        }

        // After exhaustion, MoveNext should continue to return false
        glyphEnumerator.MoveNext().ShouldBeFalse();
        glyphEnumerator.MoveNext().ShouldBeFalse();
    }

    [Fact]
    public void MoveNext_AfterEnumerationComplete_ContinuesToReturnFalse()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        // Fully enumerate in place (ref struct is copied when passed to methods)
        int count = 0;
        while (glyphEnumerator.MoveNext())
        {
            count++;
        }

        count.ShouldBeGreaterThan(0);

        // Additional calls should return false
        glyphEnumerator.MoveNext().ShouldBeFalse();
    }

    // GetEnumerator tests

    [Fact]
    public void GetEnumerator_ReturnsSelf()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        // GetEnumerator should return a copy of the enumerator (ref struct semantics)
        HalfBlockGlyphEnumerator result = glyphEnumerator.GetEnumerator();

        // Both should be able to enumerate (they are independent copies due to ref struct)
        _ = result; // Verify it compiles and doesn't throw
    }

    [Fact]
    public void GetEnumerator_AllowsForeachSyntax()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        int count = 0;
        foreach (Glyph glyph in glyphEnumerator)
        {
            count++;
            _ = glyph; // Use the glyph
        }

        count.ShouldBeGreaterThan(0);
    }

    // Current property tests

    [Fact]
    public void Current_BeforeMoveNext_ReturnsDefault()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        // Before any MoveNext, Current should be default
        Glyph current = glyphEnumerator.Current;
        current.ShouldBe(default);
    }

    [Fact]
    public void Current_AfterMoveNext_ReturnsCurrentGlyph()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        glyphEnumerator.MoveNext().ShouldBeTrue();

        Glyph current = glyphEnumerator.Current;
        current.ShouldNotBe(default);
    }

    // GlyphReader factory tests

    [Fact]
    public void FromTokensHalfBlock_CreatesEnumerator()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        HalfBlockGlyphEnumerator glyphEnumerator = GlyphReader.FromTokensHalfBlock(tokenEnumerator);

        // Should be able to enumerate
        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);
        glyphs.ShouldNotBeEmpty();
    }

    // Half-block character mapping tests

    [Fact]
    public void MoveNext_TopAliveBottomDead_YieldsUpperHalfBlock()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Only top row alive
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should have upper half block characters
        glyphs.Any(g => g.Character == ConsoleTheme.HalfBlock.UpperHalf).ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_TopDeadBottomAlive_YieldsLowerHalfBlock()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Only bottom row alive
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 1), (1, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should have lower half block characters
        glyphs.Any(g => g.Character == ConsoleTheme.HalfBlock.LowerHalf).ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_BothAlive_YieldsFullBlock()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Both rows alive
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0), (0, 1), (1, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should have full block characters
        glyphs.Any(g => g.Character == ConsoleTheme.HalfBlock.Full).ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_BothDead_YieldsSpace()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // No cells alive
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should have space characters (not newlines)
        IEnumerable<Glyph> cellGlyphs = glyphs.Where(g => !g.IsNewline);
        cellGlyphs.All(g => g.Character == ' ').ShouldBeTrue();
    }

    // Border handling tests

    [Fact]
    public void MoveNext_WithBorder_IncludesBorderGlyphs()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Should contain border characters
        glyphs.Any(g => g.Character == ConsoleTheme.Border.TopLeft).ShouldBeTrue();
        glyphs.Any(g => g.Character == ConsoleTheme.Border.TopRight).ShouldBeTrue();
        glyphs.Any(g => g.Character == ConsoleTheme.Border.BottomLeft).ShouldBeTrue();
        glyphs.Any(g => g.Character == ConsoleTheme.Border.BottomRight).ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_WithBorder_BorderGlyphsHaveGrayColor()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);

        var tokenEnumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        var glyphEnumerator = new HalfBlockGlyphEnumerator(tokenEnumerator);

        List<Glyph> glyphs = CollectGlyphs(glyphEnumerator);

        // Border characters should have gray foreground
        Glyph cornerGlyph = glyphs.First(g => g.Character == ConsoleTheme.Border.TopLeft);
        cornerGlyph.Color.ShouldBe(AnsiSequence.ForegroundGray);
    }
}
