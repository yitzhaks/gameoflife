using GameOfLife.Core;
using GameOfLife.Rendering;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class RectangularBoundsTests
{
    [Fact]
    public void Constructor_ValidMinMax_CreatesBounds()
    {
        var min = new Point2D(0, 0);
        var max = new Point2D(10, 10);

        var bounds = new RectangularBounds(min, max);

        Assert.Equal(min, bounds.Min);
        Assert.Equal(max, bounds.Max);
    }

    [Fact]
    public void Constructor_MinEqualsMax_CreatesBounds()
    {
        var point = new Point2D(5, 5);

        var bounds = new RectangularBounds(point, point);

        Assert.Equal(point, bounds.Min);
        Assert.Equal(point, bounds.Max);
    }

    [Fact]
    public void Constructor_MinXGreaterThanMaxX_ThrowsArgumentException()
    {
        var min = new Point2D(10, 0);
        var max = new Point2D(5, 10);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new RectangularBounds(min, max));
        Assert.Contains("Min", exception.Message);
    }

    [Fact]
    public void Constructor_MinYGreaterThanMaxY_ThrowsArgumentException()
    {
        var min = new Point2D(0, 10);
        var max = new Point2D(10, 5);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => new RectangularBounds(min, max));
        Assert.Contains("Min", exception.Message);
    }

    [Fact]
    public void Contains_PointInsideBounds_ReturnsTrue()
    {
        var bounds = new RectangularBounds(new Point2D(0, 0), new Point2D(10, 10));

        Assert.True(bounds.Contains(new Point2D(5, 5)));
    }

    [Fact]
    public void Contains_PointAtMinCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds(new Point2D(0, 0), new Point2D(10, 10));

        Assert.True(bounds.Contains(new Point2D(0, 0)));
    }

    [Fact]
    public void Contains_PointAtMaxCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds(new Point2D(0, 0), new Point2D(10, 10));

        Assert.True(bounds.Contains(new Point2D(10, 10)));
    }

    [Fact]
    public void Contains_PointOutsideBounds_ReturnsFalse()
    {
        var bounds = new RectangularBounds(new Point2D(0, 0), new Point2D(10, 10));

        Assert.False(bounds.Contains(new Point2D(15, 5)));
    }

    [Fact]
    public void Contains_PointBelowMinX_ReturnsFalse()
    {
        var bounds = new RectangularBounds(new Point2D(0, 0), new Point2D(10, 10));

        Assert.False(bounds.Contains(new Point2D(-1, 5)));
    }

    [Fact]
    public void Contains_PointAboveMaxY_ReturnsFalse()
    {
        var bounds = new RectangularBounds(new Point2D(0, 0), new Point2D(10, 10));

        Assert.False(bounds.Contains(new Point2D(5, 11)));
    }
}
