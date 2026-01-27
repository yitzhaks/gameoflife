using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

/// <summary>
/// Tests for viewport rendering in TokenEnumerator.
/// Tests all 9 equivalence classes: TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight.
/// </summary>
public class TokenEnumeratorViewportTests
{
    // Board is 20x20, viewport is 5x3, giving us room to test all 9 positions
    private const int BoardWidth = 20;
    private const int BoardHeight = 20;
    private const int ViewportWidth = 5;
    private const int ViewportHeight = 3;

    private static string RenderToString(Viewport viewport)
    {
        var topology = new RectangularTopology((BoardWidth, BoardHeight));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                // Skip ANSI sequences for cleaner test output
                continue;
            }

            _ = sb.Append(token.Character);
        }

        return sb.ToString();
    }

    private static string[] GetLines(string output) => output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    [Fact]
    public void Viewport_TopLeft_HasSolidTopAndLeftBorders()
    {
        // Position: top-left corner (OffsetX=0, OffsetY=0)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        // Already at (0,0) - don't move

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: solid (at top edge) - corners follow top edge's solid character
        lines[0].ShouldStartWith("╔"); // Top-left corner (both at top+left)
        lines[0].ShouldContain("═════"); // Solid horizontal
        lines[0].ShouldEndWith("═"); // Top-right extends solid (at top, not at right)

        // Left border: solid vertical bars
        lines[1].ShouldStartWith("║");
        lines[2].ShouldStartWith("║");
        lines[3].ShouldStartWith("║");

        // Right border: arrows (more content right)
        lines[1].ShouldEndWith("→");
        lines[2].ShouldEndWith("→");
        lines[3].ShouldEndWith("→");

        // Bottom border: down arrows (more content below)
        lines[4].ShouldStartWith("║"); // Bottom-left corner (at left edge, not bottom)
        lines[4].ShouldContain("↓↓↓↓↓"); // Down arrows
        lines[4].ShouldEndWith("↘"); // Diagonal corner (not at bottom, not at right)
    }

    [Fact]
    public void Viewport_Top_HasSolidTopBorderWithArrowsOnSides()
    {
        // Position: top-center (OffsetX=5, OffsetY=0)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(5, 0);

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: solid (at top edge), but left corner shows more content left
        lines[0].ShouldStartWith("═"); // Top-left corner extends solid (at top, not at left)
        lines[0].ShouldContain("═════"); // Solid horizontal
        lines[0].ShouldEndWith("═"); // Top-right corner (at top, not at right)

        // Left border: arrows (more content left)
        lines[1].ShouldStartWith("←");
        lines[2].ShouldStartWith("←");
        lines[3].ShouldStartWith("←");

        // Right border: arrows (more content right)
        lines[1].ShouldEndWith("→");
        lines[2].ShouldEndWith("→");
        lines[3].ShouldEndWith("→");

        // Bottom border: arrows (more content below)
        lines[4].ShouldStartWith("↙"); // Diagonal corner (at neither edge)
        lines[4].ShouldContain("↓↓↓↓↓"); // Down arrows
        lines[4].ShouldEndWith("↘"); // Diagonal corner
    }

    [Fact]
    public void Viewport_TopRight_HasSolidTopAndRightBorders()
    {
        // Position: top-right corner (OffsetX=15, OffsetY=0)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(100, 0); // Moves to max right (15)

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: solid (at top edge)
        lines[0].ShouldStartWith("═"); // Top-left corner (at top, not at left)
        lines[0].ShouldContain("═════"); // Solid horizontal
        lines[0].ShouldEndWith("╗"); // Top-right corner (at both edges)

        // Left border: arrows (more content left)
        lines[1].ShouldStartWith("←");
        lines[2].ShouldStartWith("←");
        lines[3].ShouldStartWith("←");

        // Right border: solid (at right edge)
        lines[1].ShouldEndWith("║");
        lines[2].ShouldEndWith("║");
        lines[3].ShouldEndWith("║");

        // Bottom border: arrows (more content below)
        lines[4].ShouldStartWith("↙"); // Diagonal corner
        lines[4].ShouldContain("↓↓↓↓↓"); // Down arrows
        lines[4].ShouldEndWith("║"); // Bottom-right corner (at right, not at bottom)
    }

    [Fact]
    public void Viewport_Left_HasSolidLeftBorderWithArrowsOnTopAndBottom()
    {
        // Position: left-center (OffsetX=0, OffsetY=5)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(0, 5);

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: arrows (more content above)
        lines[0].ShouldStartWith("║"); // Top-left corner (at left, not at top)
        lines[0].ShouldContain("↑↑↑↑↑"); // Up arrows
        lines[0].ShouldEndWith("↗"); // Diagonal corner

        // Left border: solid (at left edge)
        lines[1].ShouldStartWith("║");
        lines[2].ShouldStartWith("║");
        lines[3].ShouldStartWith("║");

        // Right border: arrows (more content right)
        lines[1].ShouldEndWith("→");
        lines[2].ShouldEndWith("→");
        lines[3].ShouldEndWith("→");

        // Bottom border: arrows (more content below)
        lines[4].ShouldStartWith("║"); // Bottom-left corner (at left, not at bottom)
        lines[4].ShouldContain("↓↓↓↓↓"); // Down arrows
        lines[4].ShouldEndWith("↘"); // Diagonal corner
    }

    [Fact]
    public void Viewport_Center_HasAllArrowBorders()
    {
        // Position: center (OffsetX=5, OffsetY=5)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(5, 5);

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: all arrows (more content above)
        lines[0].ShouldStartWith("↖"); // Diagonal corner (at neither edge)
        lines[0].ShouldContain("↑↑↑↑↑"); // Up arrows
        lines[0].ShouldEndWith("↗"); // Diagonal corner

        // Left border: arrows (more content left)
        lines[1].ShouldStartWith("←");
        lines[2].ShouldStartWith("←");
        lines[3].ShouldStartWith("←");

        // Right border: arrows (more content right)
        lines[1].ShouldEndWith("→");
        lines[2].ShouldEndWith("→");
        lines[3].ShouldEndWith("→");

        // Bottom border: all arrows (more content below)
        lines[4].ShouldStartWith("↙"); // Diagonal corner
        lines[4].ShouldContain("↓↓↓↓↓"); // Down arrows
        lines[4].ShouldEndWith("↘"); // Diagonal corner
    }

    [Fact]
    public void Viewport_Right_HasSolidRightBorderWithArrowsOnTopAndBottom()
    {
        // Position: right-center (OffsetX=15, OffsetY=5)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(100, 5); // Moves to max right (15)

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: arrows (more content above)
        lines[0].ShouldStartWith("↖"); // Diagonal corner
        lines[0].ShouldContain("↑↑↑↑↑"); // Up arrows
        lines[0].ShouldEndWith("║"); // Top-right corner (at right, not at top)

        // Left border: arrows (more content left)
        lines[1].ShouldStartWith("←");
        lines[2].ShouldStartWith("←");
        lines[3].ShouldStartWith("←");

        // Right border: solid (at right edge)
        lines[1].ShouldEndWith("║");
        lines[2].ShouldEndWith("║");
        lines[3].ShouldEndWith("║");

        // Bottom border: arrows (more content below)
        lines[4].ShouldStartWith("↙"); // Diagonal corner
        lines[4].ShouldContain("↓↓↓↓↓"); // Down arrows
        lines[4].ShouldEndWith("║"); // Bottom-right corner (at right, not at bottom)
    }

    [Fact]
    public void Viewport_BottomLeft_HasSolidBottomAndLeftBorders()
    {
        // Position: bottom-left corner (OffsetX=0, OffsetY=17)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(0, 100); // Moves to max bottom (17)

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: arrows (more content above)
        lines[0].ShouldStartWith("║"); // Top-left corner (at left, not at top)
        lines[0].ShouldContain("↑↑↑↑↑"); // Up arrows
        lines[0].ShouldEndWith("↗"); // Diagonal corner

        // Left border: solid (at left edge)
        lines[1].ShouldStartWith("║");
        lines[2].ShouldStartWith("║");
        lines[3].ShouldStartWith("║");

        // Right border: arrows (more content right)
        lines[1].ShouldEndWith("→");
        lines[2].ShouldEndWith("→");
        lines[3].ShouldEndWith("→");

        // Bottom border: solid (at bottom edge)
        lines[4].ShouldStartWith("╚"); // Bottom-left corner (at both edges)
        lines[4].ShouldContain("═════"); // Solid horizontal
        lines[4].ShouldEndWith("═"); // Bottom-right corner (at bottom, not at right)
    }

    [Fact]
    public void Viewport_Bottom_HasSolidBottomBorderWithArrowsOnSides()
    {
        // Position: bottom-center (OffsetX=5, OffsetY=17)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(5, 100); // Moves to max bottom (17)

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: arrows (more content above)
        lines[0].ShouldStartWith("↖"); // Diagonal corner
        lines[0].ShouldContain("↑↑↑↑↑"); // Up arrows
        lines[0].ShouldEndWith("↗"); // Diagonal corner

        // Left border: arrows (more content left)
        lines[1].ShouldStartWith("←");
        lines[2].ShouldStartWith("←");
        lines[3].ShouldStartWith("←");

        // Right border: arrows (more content right)
        lines[1].ShouldEndWith("→");
        lines[2].ShouldEndWith("→");
        lines[3].ShouldEndWith("→");

        // Bottom border: solid (at bottom edge)
        lines[4].ShouldStartWith("═"); // Bottom-left corner (at bottom, not at left)
        lines[4].ShouldContain("═════"); // Solid horizontal
        lines[4].ShouldEndWith("═"); // Bottom-right corner (at bottom, not at right)
    }

    [Fact]
    public void Viewport_BottomRight_HasSolidBottomAndRightBorders()
    {
        // Position: bottom-right corner (OffsetX=15, OffsetY=17)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(100, 100); // Moves to max corner (15, 17)

        string output = RenderToString(viewport);
        string[] lines = GetLines(output);

        // Top border: arrows (more content above)
        lines[0].ShouldStartWith("↖"); // Diagonal corner
        lines[0].ShouldContain("↑↑↑↑↑"); // Up arrows
        lines[0].ShouldEndWith("║"); // Top-right corner (at right, not at top)

        // Left border: arrows (more content left)
        lines[1].ShouldStartWith("←");
        lines[2].ShouldStartWith("←");
        lines[3].ShouldStartWith("←");

        // Right border: solid (at right edge)
        lines[1].ShouldEndWith("║");
        lines[2].ShouldEndWith("║");
        lines[3].ShouldEndWith("║");

        // Bottom border: solid (at bottom edge)
        lines[4].ShouldStartWith("═"); // Bottom-left corner (at bottom, not at left)
        lines[4].ShouldContain("═════"); // Solid horizontal
        lines[4].ShouldEndWith("╝"); // Bottom-right corner (at both edges)
    }

    [Fact]
    public void Viewport_NoViewport_UsesClassicBorders()
    {
        // When viewport is null, all borders should be solid
        var topology = new RectangularTopology((5, 3));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport: null);

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
        string[] lines = GetLines(output);

        // All classic solid borders
        lines[0].ShouldBe("╔═════╗");
        lines[1].ShouldBe("║.....║");
        lines[2].ShouldBe("║.....║");
        lines[3].ShouldBe("║.....║");
        lines[4].ShouldBe("╚═════╝");
    }

    [Fact]
    public void Viewport_CellClipping_OnlyRendersVisibleCells()
    {
        // Create a board with a pattern, verify only viewport cells are rendered
        var topology = new RectangularTopology((10, 10));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        // Put alive cells in a specific pattern
        var states = new Dictionary<Point2D, bool>
        {
            [(2, 2)] = true,
            [(3, 2)] = true,
            [(4, 2)] = true,
            [(7, 7)] = true,
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        // Viewport showing (2,2) to (4,4) - should see the three alive cells
        var viewport = new Viewport(3, 3, 10, 10);
        viewport.Move(2, 2);

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
        string[] lines = GetLines(output);

        // Should show 3x3 grid with top row all alive
        lines.Length.ShouldBe(3);
        lines[0].ShouldBe("###"); // Row at y=2: cells at x=2,3,4 are alive
        lines[1].ShouldBe("..."); // Row at y=3: all dead
        lines[2].ShouldBe("..."); // Row at y=4: all dead
    }
}
