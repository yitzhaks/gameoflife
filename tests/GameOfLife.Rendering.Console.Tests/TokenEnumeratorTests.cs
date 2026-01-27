using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

/// <summary>
/// Unit tests for TokenEnumerator covering argument validation, non-topology points,
/// color optimization, and edge cases not covered by TokenEnumeratorViewportTests.
/// </summary>
public class TokenEnumeratorTests
{
    [Fact]
    public void Constructor_NullLayout_ThrowsArgumentNullException()
    {
        var topology = new RectangularTopology((3, 3));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        ConsoleTheme theme = ConsoleTheme.Default;

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            _ = new TokenEnumerator(null!, generation, nodeSet, theme));

        exception.ParamName.ShouldBe("layout");
    }

    [Fact]
    public void Constructor_NullTheme_ThrowsArgumentNullException()
    {
        var topology = new RectangularTopology((3, 3));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            _ = new TokenEnumerator(layout, generation, nodeSet, null!));

        exception.ParamName.ShouldBe("theme");
    }

    [Fact]
    public void MoveNext_NonTopologyPoints_RendersSpaceWithoutColorChange()
    {
        // Create a sparse topology that doesn't include all points in the rectangular bounds
        // We'll simulate this by using a partial node set
        var topology = new RectangularTopology((3, 3));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        // Only include corner points, leaving center and edge midpoints out
        var nodeSet = new HashSet<Point2D>
        {
            (0, 0), (2, 0),  // Top corners
            (0, 2), (2, 2),  // Bottom corners
        };

        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Non-topology points render as space, topology points as dead char
        lines.Length.ShouldBe(3);
        lines[0].ShouldBe(". ."); // (0,0) dead, (1,0) non-topo space, (2,0) dead
        lines[1].ShouldBe("   "); // All middle row are non-topo
        lines[2].ShouldBe(". ."); // (0,2) dead, (1,2) non-topo space, (2,2) dead
    }

    [Fact]
    public void MoveNext_ConsecutiveSameColorCells_SkipsRedundantColorSequences()
    {
        var topology = new RectangularTopology((3, 1));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        // All cells dead - should emit color once, not three times
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        var tokens = new List<Token>();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        // Count color sequences for dead cells (DarkGray)
        int darkGrayCount = tokens.Count(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundDarkGray);

        // Should only emit color once for all consecutive dead cells
        darkGrayCount.ShouldBe(1);
    }

    [Fact]
    public void MoveNext_AlternatingAliveDead_EmitsColorForEachChange()
    {
        var topology = new RectangularTopology((3, 1));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        // Alternating: alive, dead, alive
        var states = new Dictionary<Point2D, bool>
        {
            [(0, 0)] = true,
            [(1, 0)] = false,
            [(2, 0)] = true,
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        var tokens = new List<Token>();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        // Count color sequences
        int greenCount = tokens.Count(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundGreen);
        int darkGrayCount = tokens.Count(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundDarkGray);

        // Should emit green twice (first and third cell) and darkgray once (second cell)
        greenCount.ShouldBe(2);
        darkGrayCount.ShouldBe(1);
    }

    [Fact]
    public void MoveNext_NoBorder_SkipsBorderPhases()
    {
        var topology = new RectangularTopology((2, 2));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();

        // Should not contain any border characters
        output.ShouldNotContain("╔");
        output.ShouldNotContain("╗");
        output.ShouldNotContain("╚");
        output.ShouldNotContain("╝");
        output.ShouldNotContain("═");
        output.ShouldNotContain("║");
    }

    [Fact]
    public void MoveNext_WithBorder_IncludesAllBorderElements()
    {
        var topology = new RectangularTopology((2, 2));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();

        // Should contain all border corners
        output.ShouldContain("╔");
        output.ShouldContain("╗");
        output.ShouldContain("╚");
        output.ShouldContain("╝");
        // Should contain horizontal and vertical elements
        output.ShouldContain("═");
        output.ShouldContain("║");
    }

    [Fact]
    public void GetEnumerator_ReturnsSelf()
    {
        var topology = new RectangularTopology((2, 2));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        // GetEnumerator should return the same enumerator (for foreach support)
        TokenEnumerator self = enumerator.GetEnumerator();

        // Both should be able to enumerate
        int count = 0;
        while (self.MoveNext())
        {
            count++;
        }

        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void MoveNext_ViewportSmallerThanBoard_ClipsToViewportBounds()
    {
        var topology = new RectangularTopology((10, 10));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        // Create alive cells outside viewport
        var states = new Dictionary<Point2D, bool>
        {
            [(0, 0)] = true, // Inside viewport
            [(9, 9)] = true, // Outside viewport
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        // Viewport showing only top-left 3x3
        var viewport = new Viewport(3, 3, 10, 10);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should only render 3x3 area
        lines.Length.ShouldBe(3);
        foreach (string line in lines)
        {
            line.Length.ShouldBe(3);
        }

        // First cell should be alive
        lines[0].ShouldStartWith("#");
        // Cell at (9,9) should not appear (outside viewport)
        output.Count(c => c == '#').ShouldBe(1);
    }

    [Fact]
    public void MoveNext_ViewportWithOffset_StartsAtCorrectPosition()
    {
        var topology = new RectangularTopology((10, 10));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        // Create alive cell at (5,5)
        var states = new Dictionary<Point2D, bool>
        {
            [(5, 5)] = true,
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        // Viewport positioned to show (5,5) at center
        var viewport = new Viewport(3, 3, 10, 10);
        viewport.Move(4, 4); // Now showing (4,4) to (6,6)

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // (5,5) should be at position (1,1) in the 3x3 viewport output
        lines.Length.ShouldBe(3);
        lines[0].ShouldBe("..."); // Row at y=4
        lines[1].ShouldBe(".#."); // Row at y=5, alive at x=5 (middle)
        lines[2].ShouldBe("..."); // Row at y=6
    }

    [Fact]
    public void MoveNext_BorderColorChanges_EmitsColorSequencesForBorders()
    {
        var topology = new RectangularTopology((2, 2));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var tokens = new List<Token>();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        // Should have gray color sequence for borders
        bool hasGraySequence = tokens.Any(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundGray);
        hasGraySequence.ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_ViewportNotAtEdge_UsesDarkGrayBorderColor()
    {
        var topology = new RectangularTopology((20, 20));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        // Viewport at center - all borders should be dark gray (indicating more content)
        var viewport = new Viewport(5, 5, 20, 20);
        viewport.Move(5, 5);

        var tokens = new List<Token>();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);

        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        // Should have dark gray color sequence for viewport borders
        bool hasDarkGraySequence = tokens.Any(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundDarkGray);
        hasDarkGraySequence.ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_SingleRowGrid_RendersCorrectly()
    {
        var topology = new RectangularTopology((5, 1));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        lines.Length.ShouldBe(3); // Top border, 1 row, bottom border
        lines[0].ShouldBe("╔═════╗");
        lines[1].ShouldBe("║.....║");
        lines[2].ShouldBe("╚═════╝");
    }

    [Fact]
    public void MoveNext_SingleColumnGrid_RendersCorrectly()
    {
        var topology = new RectangularTopology((1, 5));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        lines.Length.ShouldBe(7); // Top border, 5 rows, bottom border
        lines[0].ShouldBe("╔═╗");
        for (int i = 1; i <= 5; i++)
        {
            lines[i].ShouldBe("║.║");
        }

        lines[6].ShouldBe("╚═╝");
    }

    [Fact]
    public void MoveNext_AllAliveCells_EmitsGreenColorOnce()
    {
        var topology = new RectangularTopology((3, 1));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        // All cells alive
        var states = new Dictionary<Point2D, bool>();
        foreach (Point2D node in topology.Nodes)
        {
            states[node] = true;
        }

        using var generation = new DictionaryGeneration<Point2D, bool>(states, false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        var tokens = new List<Token>();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        // Count green color sequences
        int greenCount = tokens.Count(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundGreen);

        // Should only emit green once for all consecutive alive cells
        greenCount.ShouldBe(1);
    }

    [Fact]
    public void MoveNext_MultipleRowsNoBorder_TransitionsCorrectlyBetweenRows()
    {
        // Test the row newline transition without borders
        var topology = new RectangularTopology((2, 3));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should have 3 rows
        lines.Length.ShouldBe(3);
        foreach (string line in lines)
        {
            line.ShouldBe("..");
        }
    }

    [Fact]
    public void MoveNext_MultipleRowsWithBorder_TransitionsCorrectlyBetweenRows()
    {
        // Test the row newline transition with borders
        var topology = new RectangularTopology((2, 3));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should have 5 lines: top border, 3 content rows, bottom border
        lines.Length.ShouldBe(5);
        lines[0].ShouldBe("╔══╗");
        lines[1].ShouldBe("║..║");
        lines[2].ShouldBe("║..║");
        lines[3].ShouldBe("║..║");
        lines[4].ShouldBe("╚══╝");
    }

    [Fact]
    public void MoveNext_SingleCellGrid_RendersCorrectly()
    {
        var topology = new RectangularTopology((1, 1));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        lines.Length.ShouldBe(3);
        lines[0].ShouldBe("╔═╗");
        lines[1].ShouldBe("║#║");
        lines[2].ShouldBe("╚═╝");
    }

    [Fact]
    public void MoveNext_ViewportLargerThanBoard_RendersFullBoard()
    {
        // Test when viewport is larger than the board
        var topology = new RectangularTopology((3, 3));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        // Viewport larger than board (10x10 viewport for 3x3 board)
        var viewport = new Viewport(10, 10, 3, 3);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should render full 3x3 board (clamped to board bounds)
        lines.Length.ShouldBe(3);
        foreach (string line in lines)
        {
            line.Length.ShouldBe(3);
        }
    }

    [Fact]
    public void MoveNext_ViewportMovedToRightEdge_RendersCorrectly()
    {
        // Test when viewport is moved to the right edge of the board
        var topology = new RectangularTopology((5, 5));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        // 3x3 viewport moved to right edge (OffsetX clamped to 2)
        var viewport = new Viewport(3, 3, 5, 5);
        viewport.Move(100, 1); // Move far right, will be clamped

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            _ = sb.Append(token.Character);
        }

        string output = sb.ToString();
        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should render 3 columns x 3 rows
        lines.Length.ShouldBe(3);
        foreach (string line in lines)
        {
            line.Length.ShouldBe(3);
        }
    }

    [Fact]
    public void MoveNext_BorderColorAfterCellColor_TransitionsCorrectly()
    {
        // Test that border color transitions correctly after cell content
        var topology = new RectangularTopology((2, 1));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 0)] = true },
            false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var tokens = new List<Token>();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);

        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        // Should have color transitions: gray (border) -> darkgray (dead) -> green (alive) -> gray (border)
        bool hasGray = tokens.Any(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundGray);
        bool hasDarkGray = tokens.Any(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundDarkGray);
        bool hasGreen = tokens.Any(t => t.IsSequence && t.Sequence == AnsiSequence.ForegroundGreen);

        hasGray.ShouldBeTrue();
        hasDarkGray.ShouldBeTrue();
        hasGreen.ShouldBeTrue();
    }
}
