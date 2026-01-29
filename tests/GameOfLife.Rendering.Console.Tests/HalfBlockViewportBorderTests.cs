using System.Text;

using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

/// <summary>
/// Tests all 24 possible viewport border element states for the half-block renderer.
/// Each test renders a 4×4 viewport (4 columns × 4 packed rows) on an 8×8 packed board
/// (topology 8×16) with all dead cells, and compares the full character output to a
/// hardcoded expected string.
///
/// The 24 cases are:
///   4 corners × 4 states each = 16  (each corner depends on two adjacent edge flags)
///   4 edges   × 2 states each =  8  (at-edge → solid, not-at-edge → arrow)
///   Total                      = 24
///
/// Four viewport positions cover all 24 cases:
///   (0,0): isAtTop=T, isAtBottom=F, isAtLeft=T, isAtRight=F
///   (4,0): isAtTop=T, isAtBottom=F, isAtLeft=F, isAtRight=T
///   (0,4): isAtTop=F, isAtBottom=T, isAtLeft=T, isAtRight=F
///   (4,4): isAtTop=F, isAtBottom=T, isAtLeft=F, isAtRight=T
/// </summary>
public sealed class HalfBlockViewportBorderTests : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    // Viewport position (0,0): Top=T, Bottom=F, Left=T, Right=F
    //
    // ╔═════
    // ║    →
    // ║    →
    // ║    →
    // ║    →
    // ║↓↓↓↓↘
    //
    private const string Expected_0_0 =
        "╔═════\n" +
        "║    →\n" +
        "║    →\n" +
        "║    →\n" +
        "║    →\n" +
        "║↓↓↓↓↘\n";

    // Viewport position (4,0): Top=T, Bottom=F, Left=F, Right=T
    //
    // ═════╗
    // ←    ║
    // ←    ║
    // ←    ║
    // ←    ║
    // ↙↓↓↓↓║
    //
    private const string Expected_4_0 =
        "═════╗\n" +
        "←    ║\n" +
        "←    ║\n" +
        "←    ║\n" +
        "←    ║\n" +
        "↙↓↓↓↓║\n";

    // Viewport position (0,4): Top=F, Bottom=T, Left=T, Right=F
    //
    // ║↑↑↑↑↗
    // ║    →
    // ║    →
    // ║    →
    // ║    →
    // ╚═════
    //
    private const string Expected_0_4 =
        "║↑↑↑↑↗\n" +
        "║    →\n" +
        "║    →\n" +
        "║    →\n" +
        "║    →\n" +
        "╚═════\n";

    // Viewport position (4,4): Top=F, Bottom=T, Left=F, Right=T
    //
    // ↖↑↑↑↑║
    // ←    ║
    // ←    ║
    // ←    ║
    // ←    ║
    // ═════╝
    //
    private const string Expected_4_4 =
        "↖↑↑↑↑║\n" +
        "←    ║\n" +
        "←    ║\n" +
        "←    ║\n" +
        "←    ║\n" +
        "═════╝\n";

    public void Dispose()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    private string RenderBorder(int moveX, int moveY)
    {
        var topology = new RectangularTopology((8, 16));
        var layout = new HalfBlockLayout(topology);
        using var builder = new RectangularGenerationBuilder((8, 16));
        IGeneration<Point2D, bool> generation = builder.Build();
        if (generation is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(4, 4, 8, 8);
        viewport.Move(moveX, moveY);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
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

    #region Top-Left Corner (4 states)

    [Fact]
    public void TopLeftCorner_AtTopAndLeft_RendersSolidCorner()
    {
        // isAtTop=T, isAtLeft=T → ╔
        string result = RenderBorder(0, 0);
        result.ShouldBe(Expected_0_0);
    }

    [Fact]
    public void TopLeftCorner_AtTopNotAtLeft_RendersHorizontal()
    {
        // isAtTop=T, isAtLeft=F → ═ (continues the top edge)
        string result = RenderBorder(4, 0);
        result.ShouldBe(Expected_4_0);
    }

    [Fact]
    public void TopLeftCorner_NotAtTopAtLeft_RendersVertical()
    {
        // isAtTop=F, isAtLeft=T → ║ (continues the left edge)
        string result = RenderBorder(0, 4);
        result.ShouldBe(Expected_0_4);
    }

    [Fact]
    public void TopLeftCorner_NotAtTopNotAtLeft_RendersDiagonalTopLeft()
    {
        // isAtTop=F, isAtLeft=F → ↖
        string result = RenderBorder(4, 4);
        result.ShouldBe(Expected_4_4);
    }

    #endregion

    #region Top-Right Corner (4 states)

    [Fact]
    public void TopRightCorner_AtTopAndRight_RendersSolidCorner()
    {
        // isAtTop=T, isAtRight=T → ╗
        string result = RenderBorder(4, 0);
        result.ShouldBe(Expected_4_0);
    }

    [Fact]
    public void TopRightCorner_AtTopNotAtRight_RendersHorizontal()
    {
        // isAtTop=T, isAtRight=F → ═ (continues the top edge)
        string result = RenderBorder(0, 0);
        result.ShouldBe(Expected_0_0);
    }

    [Fact]
    public void TopRightCorner_NotAtTopAtRight_RendersVertical()
    {
        // isAtTop=F, isAtRight=T → ║ (continues the right edge)
        string result = RenderBorder(4, 4);
        result.ShouldBe(Expected_4_4);
    }

    [Fact]
    public void TopRightCorner_NotAtTopNotAtRight_RendersDiagonalTopRight()
    {
        // isAtTop=F, isAtRight=F → ↗
        string result = RenderBorder(0, 4);
        result.ShouldBe(Expected_0_4);
    }

    #endregion

    #region Bottom-Left Corner (4 states)

    [Fact]
    public void BottomLeftCorner_AtBottomAndLeft_RendersSolidCorner()
    {
        // isAtBottom=T, isAtLeft=T → ╚
        string result = RenderBorder(0, 4);
        result.ShouldBe(Expected_0_4);
    }

    [Fact]
    public void BottomLeftCorner_AtBottomNotAtLeft_RendersHorizontal()
    {
        // isAtBottom=T, isAtLeft=F → ═ (continues the bottom edge)
        string result = RenderBorder(4, 4);
        result.ShouldBe(Expected_4_4);
    }

    [Fact]
    public void BottomLeftCorner_NotAtBottomAtLeft_RendersVertical()
    {
        // isAtBottom=F, isAtLeft=T → ║ (continues the left edge)
        string result = RenderBorder(0, 0);
        result.ShouldBe(Expected_0_0);
    }

    [Fact]
    public void BottomLeftCorner_NotAtBottomNotAtLeft_RendersDiagonalBottomLeft()
    {
        // isAtBottom=F, isAtLeft=F → ↙
        string result = RenderBorder(4, 0);
        result.ShouldBe(Expected_4_0);
    }

    #endregion

    #region Bottom-Right Corner (4 states)

    [Fact]
    public void BottomRightCorner_AtBottomAndRight_RendersSolidCorner()
    {
        // isAtBottom=T, isAtRight=T → ╝
        string result = RenderBorder(4, 4);
        result.ShouldBe(Expected_4_4);
    }

    [Fact]
    public void BottomRightCorner_AtBottomNotAtRight_RendersHorizontal()
    {
        // isAtBottom=T, isAtRight=F → ═ (continues the bottom edge)
        string result = RenderBorder(0, 4);
        result.ShouldBe(Expected_0_4);
    }

    [Fact]
    public void BottomRightCorner_NotAtBottomAtRight_RendersVertical()
    {
        // isAtBottom=F, isAtRight=T → ║ (continues the right edge)
        string result = RenderBorder(4, 0);
        result.ShouldBe(Expected_4_0);
    }

    [Fact]
    public void BottomRightCorner_NotAtBottomNotAtRight_RendersDiagonalBottomRight()
    {
        // isAtBottom=F, isAtRight=F → ↘
        string result = RenderBorder(0, 0);
        result.ShouldBe(Expected_0_0);
    }

    #endregion

    #region Top Edge (2 states)

    [Fact]
    public void TopEdge_AtTop_RendersSolidHorizontal()
    {
        // isAtTop=T → ════ between corners
        string result = RenderBorder(0, 0);
        result.ShouldBe(Expected_0_0);
    }

    [Fact]
    public void TopEdge_NotAtTop_RendersUpArrows()
    {
        // isAtTop=F → ↑↑↑↑ between corners
        string result = RenderBorder(0, 4);
        result.ShouldBe(Expected_0_4);
    }

    #endregion

    #region Bottom Edge (2 states)

    [Fact]
    public void BottomEdge_AtBottom_RendersSolidHorizontal()
    {
        // isAtBottom=T → ════ between corners
        string result = RenderBorder(0, 4);
        result.ShouldBe(Expected_0_4);
    }

    [Fact]
    public void BottomEdge_NotAtBottom_RendersDownArrows()
    {
        // isAtBottom=F → ↓↓↓↓ between corners
        string result = RenderBorder(0, 0);
        result.ShouldBe(Expected_0_0);
    }

    #endregion

    #region Left Edge (2 states)

    [Fact]
    public void LeftEdge_AtLeft_RendersSolidVertical()
    {
        // isAtLeft=T → ║ on each content row
        string result = RenderBorder(0, 0);
        result.ShouldBe(Expected_0_0);
    }

    [Fact]
    public void LeftEdge_NotAtLeft_RendersLeftArrows()
    {
        // isAtLeft=F → ← on each content row
        string result = RenderBorder(4, 0);
        result.ShouldBe(Expected_4_0);
    }

    #endregion

    #region Right Edge (2 states)

    [Fact]
    public void RightEdge_AtRight_RendersSolidVertical()
    {
        // isAtRight=T → ║ on each content row
        string result = RenderBorder(4, 0);
        result.ShouldBe(Expected_4_0);
    }

    [Fact]
    public void RightEdge_NotAtRight_RendersRightArrows()
    {
        // isAtRight=F → → on each content row
        string result = RenderBorder(0, 0);
        result.ShouldBe(Expected_0_0);
    }

    #endregion
}
