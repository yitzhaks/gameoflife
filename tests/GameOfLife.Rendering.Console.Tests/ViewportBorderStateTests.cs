using System.Text;

using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

/// <summary>
/// Tests all 16 possible viewport border state combinations for the TokenEnumerator.
/// Each test renders a 4×4 viewport with all dead cells (shown as '.') and compares
/// the full character output to a hardcoded expected string.
///
/// The 16 cases are all combinations of: isAtTop × isAtBottom × isAtLeft × isAtRight.
/// Board size and viewport offset are chosen per-test to achieve each combination.
/// </summary>
public sealed class ViewportBorderStateTests
{
    private static string ExtractCharacters(TokenEnumerator enumerator)
    {
        var sb = new StringBuilder();
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsCharacter)
            {
                _ = sb.Append(token.Character);
            }
        }

        return sb.ToString();
    }

    [Fact]
    public void Border_AllEdges()
    {
        // 4×4 board, 4×4 viewport → at all edges
        var topology = new RectangularTopology((4, 4));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 4, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "╔════╗\n" +
            "║....║\n" +
            "║....║\n" +
            "║....║\n" +
            "║....║\n" +
            "╚════╝\n");
    }

    [Fact]
    public void Border_AtTopBottomLeft_NotAtRight()
    {
        // 8×4 board, 4×4 viewport at (0,0) → at top, bottom, left; not at right
        var topology = new RectangularTopology((8, 4));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "╔═════\n" +
            "║....→\n" +
            "║....→\n" +
            "║....→\n" +
            "║....→\n" +
            "╚═════\n");
    }

    [Fact]
    public void Border_AtTopBottomRight_NotAtLeft()
    {
        // 8×4 board, 4×4 viewport at (4,0) → at top, bottom, right; not at left
        var topology = new RectangularTopology((8, 4));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 4);
        viewport.Move(4, 0);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "═════╗\n" +
            "←....║\n" +
            "←....║\n" +
            "←....║\n" +
            "←....║\n" +
            "═════╝\n");
    }

    [Fact]
    public void Border_AtTopBottom_NotAtLeftRight()
    {
        // 12×4 board, 4×4 viewport at (4,0) → at top, bottom; not at left, right
        var topology = new RectangularTopology((12, 4));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 12, 4);
        viewport.Move(4, 0);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "══════\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "══════\n");
    }

    [Fact]
    public void Border_AtTopLeftRight_NotAtBottom()
    {
        // 4×8 board, 4×4 viewport at (0,0) → at top, left, right; not at bottom
        var topology = new RectangularTopology((4, 8));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 4, 8);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "╔════╗\n" +
            "║....║\n" +
            "║....║\n" +
            "║....║\n" +
            "║....║\n" +
            "║↓↓↓↓║\n");
    }

    [Fact]
    public void Border_AtTopLeft_NotAtBottomRight()
    {
        // 8×8 board, 4×4 viewport at (0,0) → at top, left; not at bottom, right
        var topology = new RectangularTopology((8, 8));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 8);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "╔═════\n" +
            "║....→\n" +
            "║....→\n" +
            "║....→\n" +
            "║....→\n" +
            "║↓↓↓↓↘\n");
    }

    [Fact]
    public void Border_AtTopRight_NotAtBottomLeft()
    {
        // 8×8 board, 4×4 viewport at (4,0) → at top, right; not at bottom, left
        var topology = new RectangularTopology((8, 8));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 8);
        viewport.Move(4, 0);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "═════╗\n" +
            "←....║\n" +
            "←....║\n" +
            "←....║\n" +
            "←....║\n" +
            "↙↓↓↓↓║\n");
    }

    [Fact]
    public void Border_AtTop_NotAtBottomLeftRight()
    {
        // 12×8 board, 4×4 viewport at (4,0) → at top; not at bottom, left, right
        var topology = new RectangularTopology((12, 8));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 12, 8);
        viewport.Move(4, 0);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "══════\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "↙↓↓↓↓↘\n");
    }

    [Fact]
    public void Border_AtBottomLeftRight_NotAtTop()
    {
        // 4×8 board, 4×4 viewport at (0,4) → at bottom, left, right; not at top
        var topology = new RectangularTopology((4, 8));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 4, 8);
        viewport.Move(0, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "║↑↑↑↑║\n" +
            "║....║\n" +
            "║....║\n" +
            "║....║\n" +
            "║....║\n" +
            "╚════╝\n");
    }

    [Fact]
    public void Border_AtBottomLeft_NotAtTopRight()
    {
        // 8×8 board, 4×4 viewport at (0,4) → at bottom, left; not at top, right
        var topology = new RectangularTopology((8, 8));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 8);
        viewport.Move(0, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "║↑↑↑↑↗\n" +
            "║....→\n" +
            "║....→\n" +
            "║....→\n" +
            "║....→\n" +
            "╚═════\n");
    }

    [Fact]
    public void Border_AtBottomRight_NotAtTopLeft()
    {
        // 8×8 board, 4×4 viewport at (4,4) → at bottom, right; not at top, left
        var topology = new RectangularTopology((8, 8));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 8);
        viewport.Move(4, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "↖↑↑↑↑║\n" +
            "←....║\n" +
            "←....║\n" +
            "←....║\n" +
            "←....║\n" +
            "═════╝\n");
    }

    [Fact]
    public void Border_AtBottom_NotAtTopLeftRight()
    {
        // 12×8 board, 4×4 viewport at (4,4) → at bottom; not at top, left, right
        var topology = new RectangularTopology((12, 8));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 12, 8);
        viewport.Move(4, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "↖↑↑↑↑↗\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "══════\n");
    }

    [Fact]
    public void Border_AtLeftRight_NotAtTopBottom()
    {
        // 4×12 board, 4×4 viewport at (0,4) → at left, right; not at top, bottom
        var topology = new RectangularTopology((4, 12));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 4, 12);
        viewport.Move(0, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "║↑↑↑↑║\n" +
            "║....║\n" +
            "║....║\n" +
            "║....║\n" +
            "║....║\n" +
            "║↓↓↓↓║\n");
    }

    [Fact]
    public void Border_AtLeft_NotAtTopBottomRight()
    {
        // 8×12 board, 4×4 viewport at (0,4) → at left; not at top, bottom, right
        var topology = new RectangularTopology((8, 12));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 12);
        viewport.Move(0, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "║↑↑↑↑↗\n" +
            "║....→\n" +
            "║....→\n" +
            "║....→\n" +
            "║....→\n" +
            "║↓↓↓↓↘\n");
    }

    [Fact]
    public void Border_AtRight_NotAtTopBottomLeft()
    {
        // 8×12 board, 4×4 viewport at (4,4) → at right; not at top, bottom, left
        var topology = new RectangularTopology((8, 12));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 12);
        viewport.Move(4, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "↖↑↑↑↑║\n" +
            "←....║\n" +
            "←....║\n" +
            "←....║\n" +
            "←....║\n" +
            "↙↓↓↓↓║\n");
    }

    [Fact]
    public void Border_NotAtAnyEdge()
    {
        // 12×12 board, 4×4 viewport at (4,4) → not at any edge
        var topology = new RectangularTopology((12, 12));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, 12, 12);
        viewport.Move(4, 4);

        ExtractCharacters(new TokenEnumerator(layout, generation, nodeSet, theme, viewport)).ShouldBe(
            "↖↑↑↑↑↗\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "↙↓↓↓↓↘\n");
    }
}
