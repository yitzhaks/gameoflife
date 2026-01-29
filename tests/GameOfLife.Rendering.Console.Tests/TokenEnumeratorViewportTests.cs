using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

/// <summary>
/// Tests for viewport rendering in TokenEnumerator.
/// Border state tests are in ViewportBorderStateTests.
/// </summary>
public class TokenEnumeratorViewportTests
{
    private static string[] GetLines(string output) => output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

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
