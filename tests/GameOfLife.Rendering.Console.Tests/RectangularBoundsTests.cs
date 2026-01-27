using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class RectangularBoundsTests
{
    [Fact]
    public void Constructor_ValidMinMax_CreatesBounds()
    {
        Point2D min = default;
        Point2D max = (10, 10);

        var bounds = new RectangularBounds(min, max);

        bounds.Min.ShouldBe(min);
        bounds.Max.ShouldBe(max);
    }

    [Fact]
    public void Constructor_MinEqualsMax_CreatesBounds()
    {
        Point2D point = (5, 5);

        var bounds = new RectangularBounds(point, point);

        bounds.Min.ShouldBe(point);
        bounds.Max.ShouldBe(point);
    }

    [Fact]
    public void Constructor_MinXGreaterThanMaxX_ThrowsArgumentException()
    {
        Point2D min = (10, 0);
        Point2D max = (5, 10);

        ArgumentException exception = Should.Throw<ArgumentException>(() => new RectangularBounds(min, max));
        exception.Message.ShouldContain("Min");
    }

    [Fact]
    public void Constructor_MinYGreaterThanMaxY_ThrowsArgumentException()
    {
        Point2D min = (0, 10);
        Point2D max = (10, 5);

        ArgumentException exception = Should.Throw<ArgumentException>(() => new RectangularBounds(min, max));
        exception.Message.ShouldContain("Min");
    }

    [Fact]
    public void Contains_PointInsideBounds_ReturnsTrue()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        bounds.Contains((5, 5)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointAtMinCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        bounds.Contains(default).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointAtMaxCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        bounds.Contains((10, 10)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointOutsideBounds_ReturnsFalse()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        bounds.Contains((15, 5)).ShouldBeFalse();
    }

    [Fact]
    public void Contains_PointBelowMinX_ReturnsFalse()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        bounds.Contains((-1, 5)).ShouldBeFalse();
    }

    [Fact]
    public void Contains_PointAboveMaxY_ReturnsFalse()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        bounds.Contains((5, 11)).ShouldBeFalse();
    }
}
