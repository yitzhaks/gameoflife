using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

/// <summary>
/// Tests for <see cref="BorderHelper"/> static methods.
/// </summary>
public sealed class BorderHelperTests
{
    #region GetTopLeftCorner

    [Fact]
    public void GetTopLeftCorner_AtTopAndLeft_ReturnsTopLeftCorner()
    {
        BorderHelper.GetTopLeftCorner(isAtTop: true, isAtLeft: true)
            .ShouldBe(ConsoleTheme.Border.TopLeft);
    }

    [Fact]
    public void GetTopLeftCorner_AtTopNotAtLeft_ReturnsHorizontal()
    {
        BorderHelper.GetTopLeftCorner(isAtTop: true, isAtLeft: false)
            .ShouldBe(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetTopLeftCorner_NotAtTopAtLeft_ReturnsVertical()
    {
        BorderHelper.GetTopLeftCorner(isAtTop: false, isAtLeft: true)
            .ShouldBe(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetTopLeftCorner_NotAtTopNotAtLeft_ReturnsDiagonalTopLeft()
    {
        BorderHelper.GetTopLeftCorner(isAtTop: false, isAtLeft: false)
            .ShouldBe(ConsoleTheme.ViewportBorder.DiagonalTopLeft);
    }

    #endregion

    #region GetTopRightCorner

    [Fact]
    public void GetTopRightCorner_AtTopAndRight_ReturnsTopRightCorner()
    {
        BorderHelper.GetTopRightCorner(isAtTop: true, isAtRight: true)
            .ShouldBe(ConsoleTheme.Border.TopRight);
    }

    [Fact]
    public void GetTopRightCorner_AtTopNotAtRight_ReturnsHorizontal()
    {
        BorderHelper.GetTopRightCorner(isAtTop: true, isAtRight: false)
            .ShouldBe(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetTopRightCorner_NotAtTopAtRight_ReturnsVertical()
    {
        BorderHelper.GetTopRightCorner(isAtTop: false, isAtRight: true)
            .ShouldBe(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetTopRightCorner_NotAtTopNotAtRight_ReturnsDiagonalTopRight()
    {
        BorderHelper.GetTopRightCorner(isAtTop: false, isAtRight: false)
            .ShouldBe(ConsoleTheme.ViewportBorder.DiagonalTopRight);
    }

    #endregion

    #region GetBottomLeftCorner

    [Fact]
    public void GetBottomLeftCorner_AtBottomAndLeft_ReturnsBottomLeftCorner()
    {
        BorderHelper.GetBottomLeftCorner(isAtBottom: true, isAtLeft: true)
            .ShouldBe(ConsoleTheme.Border.BottomLeft);
    }

    [Fact]
    public void GetBottomLeftCorner_AtBottomNotAtLeft_ReturnsHorizontal()
    {
        BorderHelper.GetBottomLeftCorner(isAtBottom: true, isAtLeft: false)
            .ShouldBe(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetBottomLeftCorner_NotAtBottomAtLeft_ReturnsVertical()
    {
        BorderHelper.GetBottomLeftCorner(isAtBottom: false, isAtLeft: true)
            .ShouldBe(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetBottomLeftCorner_NotAtBottomNotAtLeft_ReturnsDiagonalBottomLeft()
    {
        BorderHelper.GetBottomLeftCorner(isAtBottom: false, isAtLeft: false)
            .ShouldBe(ConsoleTheme.ViewportBorder.DiagonalBottomLeft);
    }

    #endregion

    #region GetBottomRightCorner

    [Fact]
    public void GetBottomRightCorner_AtBottomAndRight_ReturnsBottomRightCorner()
    {
        BorderHelper.GetBottomRightCorner(isAtBottom: true, isAtRight: true)
            .ShouldBe(ConsoleTheme.Border.BottomRight);
    }

    [Fact]
    public void GetBottomRightCorner_AtBottomNotAtRight_ReturnsHorizontal()
    {
        BorderHelper.GetBottomRightCorner(isAtBottom: true, isAtRight: false)
            .ShouldBe(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetBottomRightCorner_NotAtBottomAtRight_ReturnsVertical()
    {
        BorderHelper.GetBottomRightCorner(isAtBottom: false, isAtRight: true)
            .ShouldBe(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetBottomRightCorner_NotAtBottomNotAtRight_ReturnsDiagonalBottomRight()
    {
        BorderHelper.GetBottomRightCorner(isAtBottom: false, isAtRight: false)
            .ShouldBe(ConsoleTheme.ViewportBorder.DiagonalBottomRight);
    }

    #endregion

    #region GetBorderColor

    [Fact]
    public void GetBorderColor_AtEdge_ReturnsGray()
    {
        BorderHelper.GetBorderColor(isAtEdge: true)
            .ShouldBe(AnsiSequence.ForegroundGray);
    }

    [Fact]
    public void GetBorderColor_NotAtEdge_ReturnsDarkGray()
    {
        BorderHelper.GetBorderColor(isAtEdge: false)
            .ShouldBe(AnsiSequence.ForegroundDarkGray);
    }

    #endregion

    #region GetHorizontalChar

    [Fact]
    public void GetHorizontalChar_AtEdgeTop_ReturnsHorizontal()
    {
        BorderHelper.GetHorizontalChar(isAtEdge: true, isTop: true)
            .ShouldBe(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetHorizontalChar_AtEdgeBottom_ReturnsHorizontal()
    {
        BorderHelper.GetHorizontalChar(isAtEdge: true, isTop: false)
            .ShouldBe(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetHorizontalChar_NotAtEdgeTop_ReturnsUpArrow()
    {
        BorderHelper.GetHorizontalChar(isAtEdge: false, isTop: true)
            .ShouldBe(ConsoleTheme.ViewportBorder.Up);
    }

    [Fact]
    public void GetHorizontalChar_NotAtEdgeBottom_ReturnsDownArrow()
    {
        BorderHelper.GetHorizontalChar(isAtEdge: false, isTop: false)
            .ShouldBe(ConsoleTheme.ViewportBorder.Down);
    }

    #endregion

    #region GetVerticalChar

    [Fact]
    public void GetVerticalChar_AtEdgeLeft_ReturnsVertical()
    {
        BorderHelper.GetVerticalChar(isAtEdge: true, isLeft: true)
            .ShouldBe(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetVerticalChar_AtEdgeRight_ReturnsVertical()
    {
        BorderHelper.GetVerticalChar(isAtEdge: true, isLeft: false)
            .ShouldBe(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetVerticalChar_NotAtEdgeLeft_ReturnsLeftArrow()
    {
        BorderHelper.GetVerticalChar(isAtEdge: false, isLeft: true)
            .ShouldBe(ConsoleTheme.ViewportBorder.Left);
    }

    [Fact]
    public void GetVerticalChar_NotAtEdgeRight_ReturnsRightArrow()
    {
        BorderHelper.GetVerticalChar(isAtEdge: false, isLeft: false)
            .ShouldBe(ConsoleTheme.ViewportBorder.Right);
    }

    #endregion
}
