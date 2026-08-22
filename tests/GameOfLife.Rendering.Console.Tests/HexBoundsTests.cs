using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class HexBoundsTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidBounds_SetsMinMax()
    {
        Point2D min = (0, 0);
        Point2D max = (10, 10);

        var bounds = new HexBounds(min, max);

        bounds.Min.ShouldBe(min);
        bounds.Max.ShouldBe(max);
    }

    [Fact]
    public void Constructor_MinEqualsMax_CreatesValidBounds()
    {
        Point2D point = (5, 5);

        var bounds = new HexBounds(point, point);

        bounds.Min.ShouldBe(point);
        bounds.Max.ShouldBe(point);
    }

    [Fact]
    public void Constructor_MinXGreaterThanMaxX_ThrowsArgumentException()
    {
        Point2D min = (10, 0);
        Point2D max = (5, 10);

        ArgumentException exception = Should.Throw<ArgumentException>(() =>
            new HexBounds(min, max));

        exception.Message.ShouldContain("Min X");
        exception.Message.ShouldContain("Max X");
    }

    [Fact]
    public void Constructor_MinYGreaterThanMaxY_ThrowsArgumentException()
    {
        Point2D min = (0, 10);
        Point2D max = (10, 5);

        ArgumentException exception = Should.Throw<ArgumentException>(() =>
            new HexBounds(min, max));

        exception.Message.ShouldContain("Min Y");
        exception.Message.ShouldContain("Max Y");
    }

    #endregion

    #region Contains Tests

    [Fact]
    public void Contains_PointInside_ReturnsTrue()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((5, 5)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointAtMin_ReturnsTrue()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((0, 0)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointAtMax_ReturnsTrue()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((10, 10)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointOnLeftEdge_ReturnsTrue()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((0, 5)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointOnRightEdge_ReturnsTrue()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((10, 5)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointOnTopEdge_ReturnsTrue()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((5, 0)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointOnBottomEdge_ReturnsTrue()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((5, 10)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointLeftOfBounds_ReturnsFalse()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((-1, 5)).ShouldBeFalse();
    }

    [Fact]
    public void Contains_PointRightOfBounds_ReturnsFalse()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((11, 5)).ShouldBeFalse();
    }

    [Fact]
    public void Contains_PointAboveBounds_ReturnsFalse()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((5, -1)).ShouldBeFalse();
    }

    [Fact]
    public void Contains_PointBelowBounds_ReturnsFalse()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        bounds.Contains((5, 11)).ShouldBeFalse();
    }

    #endregion

    #region ForHexLayout Tests

    [Fact]
    public void ForHexLayout_RadiusZero_CalculatesCorrectBounds()
    {
        var bounds = HexBounds.ForHexLayout(0);

        // Radius 0: 1 row, 2 chars (1 cell * 2)
        bounds.Min.ShouldBe(default);
        bounds.Max.X.ShouldBe(1); // width - 1 = 2 - 1 = 1
        bounds.Max.Y.ShouldBe(0); // height - 1 = 1 - 1 = 0
    }

    [Fact]
    public void ForHexLayout_RadiusOne_CalculatesCorrectBounds()
    {
        var bounds = HexBounds.ForHexLayout(1);

        // Radius 1: 3 rows, 6 chars (3 cells * 2)
        bounds.Min.ShouldBe(default);
        bounds.Max.X.ShouldBe(5); // width - 1 = 6 - 1 = 5
        bounds.Max.Y.ShouldBe(2); // height - 1 = 3 - 1 = 2
    }

    [Fact]
    public void ForHexLayout_RadiusTwo_CalculatesCorrectBounds()
    {
        var bounds = HexBounds.ForHexLayout(2);

        // Radius 2: 5 rows, 10 chars (5 cells * 2)
        bounds.Min.ShouldBe(default);
        bounds.Max.X.ShouldBe(9); // width - 1 = 10 - 1 = 9
        bounds.Max.Y.ShouldBe(4); // height - 1 = 5 - 1 = 4
    }

    #endregion

    #region IAxisAlignedBounds Interface Tests

    [Fact]
    public void HexBounds_ImplementsIAxisAlignedBounds()
    {
        var bounds = new HexBounds((0, 0), (10, 10));

        _ = bounds.ShouldBeAssignableTo<IAxisAlignedBounds<Point2D>>();
    }

    #endregion
}
