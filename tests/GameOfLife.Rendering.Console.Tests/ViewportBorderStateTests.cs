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
    private static string RenderBorder(bool atTop, bool atBottom, bool atLeft, bool atRight)
    {
        int boardW, moveX;
        if (atLeft && atRight) { boardW = 4; moveX = 0; }
        else if (atLeft) { boardW = 8; moveX = 0; }
        else if (atRight) { boardW = 8; moveX = 4; }
        else { boardW = 12; moveX = 4; }

        int boardH, moveY;
        if (atTop && atBottom) { boardH = 4; moveY = 0; }
        else if (atTop) { boardH = 8; moveY = 0; }
        else if (atBottom) { boardH = 8; moveY = 4; }
        else { boardH = 12; moveY = 4; }

        var topology = new RectangularTopology((boardW, boardH));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        using var generation = new DictionaryGeneration<Point2D, bool>(new Dictionary<Point2D, bool>(), false);
        var theme = new ConsoleTheme(DeadChar: '.', ShowBorder: true);
        var viewport = new Viewport(4, 4, boardW, boardH);
        viewport.Move(moveX, moveY);

        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme, viewport);
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
        // isAtTop=T, isAtBottom=T, isAtLeft=T, isAtRight=T
        RenderBorder(atTop: true, atBottom: true, atLeft: true, atRight: true).ShouldBe(
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
        // isAtTop=T, isAtBottom=T, isAtLeft=T, isAtRight=F
        RenderBorder(atTop: true, atBottom: true, atLeft: true, atRight: false).ShouldBe(
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
        // isAtTop=T, isAtBottom=T, isAtLeft=F, isAtRight=T
        RenderBorder(atTop: true, atBottom: true, atLeft: false, atRight: true).ShouldBe(
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
        // isAtTop=T, isAtBottom=T, isAtLeft=F, isAtRight=F
        RenderBorder(atTop: true, atBottom: true, atLeft: false, atRight: false).ShouldBe(
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
        // isAtTop=T, isAtBottom=F, isAtLeft=T, isAtRight=T
        RenderBorder(atTop: true, atBottom: false, atLeft: true, atRight: true).ShouldBe(
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
        // isAtTop=T, isAtBottom=F, isAtLeft=T, isAtRight=F
        RenderBorder(atTop: true, atBottom: false, atLeft: true, atRight: false).ShouldBe(
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
        // isAtTop=T, isAtBottom=F, isAtLeft=F, isAtRight=T
        RenderBorder(atTop: true, atBottom: false, atLeft: false, atRight: true).ShouldBe(
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
        // isAtTop=T, isAtBottom=F, isAtLeft=F, isAtRight=F
        RenderBorder(atTop: true, atBottom: false, atLeft: false, atRight: false).ShouldBe(
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
        // isAtTop=F, isAtBottom=T, isAtLeft=T, isAtRight=T
        RenderBorder(atTop: false, atBottom: true, atLeft: true, atRight: true).ShouldBe(
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
        // isAtTop=F, isAtBottom=T, isAtLeft=T, isAtRight=F
        RenderBorder(atTop: false, atBottom: true, atLeft: true, atRight: false).ShouldBe(
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
        // isAtTop=F, isAtBottom=T, isAtLeft=F, isAtRight=T
        RenderBorder(atTop: false, atBottom: true, atLeft: false, atRight: true).ShouldBe(
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
        // isAtTop=F, isAtBottom=T, isAtLeft=F, isAtRight=F
        RenderBorder(atTop: false, atBottom: true, atLeft: false, atRight: false).ShouldBe(
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
        // isAtTop=F, isAtBottom=F, isAtLeft=T, isAtRight=T
        RenderBorder(atTop: false, atBottom: false, atLeft: true, atRight: true).ShouldBe(
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
        // isAtTop=F, isAtBottom=F, isAtLeft=T, isAtRight=F
        RenderBorder(atTop: false, atBottom: false, atLeft: true, atRight: false).ShouldBe(
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
        // isAtTop=F, isAtBottom=F, isAtLeft=F, isAtRight=T
        RenderBorder(atTop: false, atBottom: false, atLeft: false, atRight: true).ShouldBe(
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
        // isAtTop=F, isAtBottom=F, isAtLeft=F, isAtRight=F
        RenderBorder(atTop: false, atBottom: false, atLeft: false, atRight: false).ShouldBe(
            "↖↑↑↑↑↗\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "←....→\n" +
            "↙↓↓↓↓↘\n");
    }
}
