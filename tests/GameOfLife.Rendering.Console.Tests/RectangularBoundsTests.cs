using GameOfLife.Core;
using GameOfLife.Rendering;

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

        Assert.Equal(min, bounds.Min);
        Assert.Equal(max, bounds.Max);
    }

    [Fact]
    public void Constructor_MinEqualsMax_CreatesBounds()
    {
        Point2D point = (5, 5);

        var bounds = new RectangularBounds(point, point);

        Assert.Equal(point, bounds.Min);
        Assert.Equal(point, bounds.Max);
    }

    [Fact]
    public void Constructor_MinXGreaterThanMaxX_ThrowsArgumentException()
    {
        Point2D min = (10, 0);
        Point2D max = (5, 10);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new RectangularBounds(min, max));
        Assert.Contains("Min", exception.Message);
    }

    [Fact]
    public void Constructor_MinYGreaterThanMaxY_ThrowsArgumentException()
    {
        Point2D min = (0, 10);
        Point2D max = (10, 5);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new RectangularBounds(min, max));
        Assert.Contains("Min", exception.Message);
    }

    [Fact]
    public void Contains_PointInsideBounds_ReturnsTrue()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        Assert.True(bounds.Contains((5, 5)));
    }

    [Fact]
    public void Contains_PointAtMinCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        Assert.True(bounds.Contains(default));
    }

    [Fact]
    public void Contains_PointAtMaxCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        Assert.True(bounds.Contains((10, 10)));
    }

    [Fact]
    public void Contains_PointOutsideBounds_ReturnsFalse()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        Assert.False(bounds.Contains((15, 5)));
    }

    [Fact]
    public void Contains_PointBelowMinX_ReturnsFalse()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        Assert.False(bounds.Contains((-1, 5)));
    }

    [Fact]
    public void Contains_PointAboveMaxY_ReturnsFalse()
    {
        var bounds = new RectangularBounds(default, (10, 10));

        Assert.False(bounds.Contains((5, 11)));
    }
}
