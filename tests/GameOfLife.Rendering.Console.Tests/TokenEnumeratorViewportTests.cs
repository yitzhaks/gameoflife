using GameOfLife.Core;
using GameOfLife.Rendering;

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
        var topology = new Grid2DTopology(BoardWidth, BoardHeight);
        var engine = new IdentityLayoutEngine();
        var layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);

        while (enumerator.MoveNext())
        {
            var token = enumerator.Current;
            if (token.IsSequence)
            {
                // Skip ANSI sequences for cleaner test output
                continue;
            }

            sb.Append(token.Character);
        }

        return sb.ToString();
    }

    private static string[] GetLines(string output)
    {
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    [Fact]
    public void Viewport_TopLeft_HasSolidTopAndLeftBorders()
    {
        // Position: top-left corner (OffsetX=0, OffsetY=0)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        // Already at (0,0) - don't move

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: solid (at top edge) - corners follow top edge's solid character
        Assert.StartsWith("╔", lines[0]); // Top-left corner (both at top+left)
        Assert.Contains("═════", lines[0]); // Solid horizontal
        Assert.EndsWith("═", lines[0]); // Top-right extends solid (at top, not at right)

        // Left border: solid vertical bars
        Assert.StartsWith("║", lines[1]);
        Assert.StartsWith("║", lines[2]);
        Assert.StartsWith("║", lines[3]);

        // Right border: arrows (more content right)
        Assert.EndsWith("→", lines[1]);
        Assert.EndsWith("→", lines[2]);
        Assert.EndsWith("→", lines[3]);

        // Bottom border: down arrows (more content below)
        Assert.StartsWith("║", lines[4]); // Bottom-left corner (at left edge, not bottom)
        Assert.Contains("↓↓↓↓↓", lines[4]); // Down arrows
        Assert.EndsWith("↘", lines[4]); // Diagonal corner (not at bottom, not at right)
    }

    [Fact]
    public void Viewport_Top_HasSolidTopBorderWithArrowsOnSides()
    {
        // Position: top-center (OffsetX=5, OffsetY=0)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(5, 0);

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: solid (at top edge), but left corner shows more content left
        Assert.StartsWith("═", lines[0]); // Top-left corner extends solid (at top, not at left)
        Assert.Contains("═════", lines[0]); // Solid horizontal
        Assert.EndsWith("═", lines[0]); // Top-right corner (at top, not at right)

        // Left border: arrows (more content left)
        Assert.StartsWith("←", lines[1]);
        Assert.StartsWith("←", lines[2]);
        Assert.StartsWith("←", lines[3]);

        // Right border: arrows (more content right)
        Assert.EndsWith("→", lines[1]);
        Assert.EndsWith("→", lines[2]);
        Assert.EndsWith("→", lines[3]);

        // Bottom border: arrows (more content below)
        Assert.StartsWith("↙", lines[4]); // Diagonal corner (at neither edge)
        Assert.Contains("↓↓↓↓↓", lines[4]); // Down arrows
        Assert.EndsWith("↘", lines[4]); // Diagonal corner
    }

    [Fact]
    public void Viewport_TopRight_HasSolidTopAndRightBorders()
    {
        // Position: top-right corner (OffsetX=15, OffsetY=0)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(100, 0); // Moves to max right (15)

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: solid (at top edge)
        Assert.StartsWith("═", lines[0]); // Top-left corner (at top, not at left)
        Assert.Contains("═════", lines[0]); // Solid horizontal
        Assert.EndsWith("╗", lines[0]); // Top-right corner (at both edges)

        // Left border: arrows (more content left)
        Assert.StartsWith("←", lines[1]);
        Assert.StartsWith("←", lines[2]);
        Assert.StartsWith("←", lines[3]);

        // Right border: solid (at right edge)
        Assert.EndsWith("║", lines[1]);
        Assert.EndsWith("║", lines[2]);
        Assert.EndsWith("║", lines[3]);

        // Bottom border: arrows (more content below)
        Assert.StartsWith("↙", lines[4]); // Diagonal corner
        Assert.Contains("↓↓↓↓↓", lines[4]); // Down arrows
        Assert.EndsWith("║", lines[4]); // Bottom-right corner (at right, not at bottom)
    }

    [Fact]
    public void Viewport_Left_HasSolidLeftBorderWithArrowsOnTopAndBottom()
    {
        // Position: left-center (OffsetX=0, OffsetY=5)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(0, 5);

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: arrows (more content above)
        Assert.StartsWith("║", lines[0]); // Top-left corner (at left, not at top)
        Assert.Contains("↑↑↑↑↑", lines[0]); // Up arrows
        Assert.EndsWith("↗", lines[0]); // Diagonal corner

        // Left border: solid (at left edge)
        Assert.StartsWith("║", lines[1]);
        Assert.StartsWith("║", lines[2]);
        Assert.StartsWith("║", lines[3]);

        // Right border: arrows (more content right)
        Assert.EndsWith("→", lines[1]);
        Assert.EndsWith("→", lines[2]);
        Assert.EndsWith("→", lines[3]);

        // Bottom border: arrows (more content below)
        Assert.StartsWith("║", lines[4]); // Bottom-left corner (at left, not at bottom)
        Assert.Contains("↓↓↓↓↓", lines[4]); // Down arrows
        Assert.EndsWith("↘", lines[4]); // Diagonal corner
    }

    [Fact]
    public void Viewport_Center_HasAllArrowBorders()
    {
        // Position: center (OffsetX=5, OffsetY=5)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(5, 5);

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: all arrows (more content above)
        Assert.StartsWith("↖", lines[0]); // Diagonal corner (at neither edge)
        Assert.Contains("↑↑↑↑↑", lines[0]); // Up arrows
        Assert.EndsWith("↗", lines[0]); // Diagonal corner

        // Left border: arrows (more content left)
        Assert.StartsWith("←", lines[1]);
        Assert.StartsWith("←", lines[2]);
        Assert.StartsWith("←", lines[3]);

        // Right border: arrows (more content right)
        Assert.EndsWith("→", lines[1]);
        Assert.EndsWith("→", lines[2]);
        Assert.EndsWith("→", lines[3]);

        // Bottom border: all arrows (more content below)
        Assert.StartsWith("↙", lines[4]); // Diagonal corner
        Assert.Contains("↓↓↓↓↓", lines[4]); // Down arrows
        Assert.EndsWith("↘", lines[4]); // Diagonal corner
    }

    [Fact]
    public void Viewport_Right_HasSolidRightBorderWithArrowsOnTopAndBottom()
    {
        // Position: right-center (OffsetX=15, OffsetY=5)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(100, 5); // Moves to max right (15)

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: arrows (more content above)
        Assert.StartsWith("↖", lines[0]); // Diagonal corner
        Assert.Contains("↑↑↑↑↑", lines[0]); // Up arrows
        Assert.EndsWith("║", lines[0]); // Top-right corner (at right, not at top)

        // Left border: arrows (more content left)
        Assert.StartsWith("←", lines[1]);
        Assert.StartsWith("←", lines[2]);
        Assert.StartsWith("←", lines[3]);

        // Right border: solid (at right edge)
        Assert.EndsWith("║", lines[1]);
        Assert.EndsWith("║", lines[2]);
        Assert.EndsWith("║", lines[3]);

        // Bottom border: arrows (more content below)
        Assert.StartsWith("↙", lines[4]); // Diagonal corner
        Assert.Contains("↓↓↓↓↓", lines[4]); // Down arrows
        Assert.EndsWith("║", lines[4]); // Bottom-right corner (at right, not at bottom)
    }

    [Fact]
    public void Viewport_BottomLeft_HasSolidBottomAndLeftBorders()
    {
        // Position: bottom-left corner (OffsetX=0, OffsetY=17)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(0, 100); // Moves to max bottom (17)

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: arrows (more content above)
        Assert.StartsWith("║", lines[0]); // Top-left corner (at left, not at top)
        Assert.Contains("↑↑↑↑↑", lines[0]); // Up arrows
        Assert.EndsWith("↗", lines[0]); // Diagonal corner

        // Left border: solid (at left edge)
        Assert.StartsWith("║", lines[1]);
        Assert.StartsWith("║", lines[2]);
        Assert.StartsWith("║", lines[3]);

        // Right border: arrows (more content right)
        Assert.EndsWith("→", lines[1]);
        Assert.EndsWith("→", lines[2]);
        Assert.EndsWith("→", lines[3]);

        // Bottom border: solid (at bottom edge)
        Assert.StartsWith("╚", lines[4]); // Bottom-left corner (at both edges)
        Assert.Contains("═════", lines[4]); // Solid horizontal
        Assert.EndsWith("═", lines[4]); // Bottom-right corner (at bottom, not at right)
    }

    [Fact]
    public void Viewport_Bottom_HasSolidBottomBorderWithArrowsOnSides()
    {
        // Position: bottom-center (OffsetX=5, OffsetY=17)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(5, 100); // Moves to max bottom (17)

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: arrows (more content above)
        Assert.StartsWith("↖", lines[0]); // Diagonal corner
        Assert.Contains("↑↑↑↑↑", lines[0]); // Up arrows
        Assert.EndsWith("↗", lines[0]); // Diagonal corner

        // Left border: arrows (more content left)
        Assert.StartsWith("←", lines[1]);
        Assert.StartsWith("←", lines[2]);
        Assert.StartsWith("←", lines[3]);

        // Right border: arrows (more content right)
        Assert.EndsWith("→", lines[1]);
        Assert.EndsWith("→", lines[2]);
        Assert.EndsWith("→", lines[3]);

        // Bottom border: solid (at bottom edge)
        Assert.StartsWith("═", lines[4]); // Bottom-left corner (at bottom, not at left)
        Assert.Contains("═════", lines[4]); // Solid horizontal
        Assert.EndsWith("═", lines[4]); // Bottom-right corner (at bottom, not at right)
    }

    [Fact]
    public void Viewport_BottomRight_HasSolidBottomAndRightBorders()
    {
        // Position: bottom-right corner (OffsetX=15, OffsetY=17)
        var viewport = new Viewport(ViewportWidth, ViewportHeight, BoardWidth, BoardHeight);
        viewport.Move(100, 100); // Moves to max corner (15, 17)

        var output = RenderToString(viewport);
        var lines = GetLines(output);

        // Top border: arrows (more content above)
        Assert.StartsWith("↖", lines[0]); // Diagonal corner
        Assert.Contains("↑↑↑↑↑", lines[0]); // Up arrows
        Assert.EndsWith("║", lines[0]); // Top-right corner (at right, not at top)

        // Left border: arrows (more content left)
        Assert.StartsWith("←", lines[1]);
        Assert.StartsWith("←", lines[2]);
        Assert.StartsWith("←", lines[3]);

        // Right border: solid (at right edge)
        Assert.EndsWith("║", lines[1]);
        Assert.EndsWith("║", lines[2]);
        Assert.EndsWith("║", lines[3]);

        // Bottom border: solid (at bottom edge)
        Assert.StartsWith("═", lines[4]); // Bottom-left corner (at bottom, not at left)
        Assert.Contains("═════", lines[4]); // Solid horizontal
        Assert.EndsWith("╝", lines[4]); // Bottom-right corner (at both edges)
    }

    [Fact]
    public void Viewport_NoViewport_UsesClassicBorders()
    {
        // When viewport is null, all borders should be solid
        var topology = new Grid2DTopology(5, 3);
        var engine = new IdentityLayoutEngine();
        var layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport: null);

        while (enumerator.MoveNext())
        {
            var token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            sb.Append(token.Character);
        }

        var output = sb.ToString();
        var lines = GetLines(output);

        // All classic solid borders
        Assert.Equal("╔═════╗", lines[0]);
        Assert.Equal("║.....║", lines[1]);
        Assert.Equal("║.....║", lines[2]);
        Assert.Equal("║.....║", lines[3]);
        Assert.Equal("╚═════╝", lines[4]);
    }

    [Fact]
    public void Viewport_CellClipping_OnlyRendersVisibleCells()
    {
        // Create a board with a pattern, verify only viewport cells are rendered
        var topology = new Grid2DTopology(10, 10);
        var engine = new IdentityLayoutEngine();
        var layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        // Put alive cells in a specific pattern
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(2, 2)] = true,
            [new Point2D(3, 2)] = true,
            [new Point2D(4, 2)] = true,
            [new Point2D(7, 7)] = true,
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, false);
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        // Viewport showing (2,2) to (4,4) - should see the three alive cells
        var viewport = new Viewport(3, 3, 10, 10);
        viewport.Move(2, 2);

        var sb = new System.Text.StringBuilder();
        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);

        while (enumerator.MoveNext())
        {
            var token = enumerator.Current;
            if (token.IsSequence)
            {
                continue;
            }

            sb.Append(token.Character);
        }

        var output = sb.ToString();
        var lines = GetLines(output);

        // Should show 3x3 grid with top row all alive
        Assert.Equal(3, lines.Length);
        Assert.Equal("###", lines[0]); // Row at y=2: cells at x=2,3,4 are alive
        Assert.Equal("...", lines[1]); // Row at y=3: all dead
        Assert.Equal("...", lines[2]); // Row at y=4: all dead
    }
}
