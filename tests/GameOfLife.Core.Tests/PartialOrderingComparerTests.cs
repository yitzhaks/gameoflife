namespace GameOfLife.Core.Tests;

public class PartialOrderingComparerTests
{
    [Fact]
    public void IsLessThan_BothDimensionsLess_ReturnsTrue()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(3, 4);

        Assert.True(a.IsLessThan(b));
    }

    [Fact]
    public void IsLessThan_OneDimensionEqual_ReturnsFalse()
    {
        var a = new Point2D(3, 2);
        var b = new Point2D(3, 4);

        Assert.False(a.IsLessThan(b));
    }

    [Fact]
    public void IsLessThan_OneDimensionGreater_ReturnsFalse()
    {
        var a = new Point2D(5, 2);
        var b = new Point2D(3, 4);

        Assert.False(a.IsLessThan(b));
    }

    [Fact]
    public void IsLessThanOrEqualTo_BothDimensionsLess_ReturnsTrue()
    {
        var a = new Point2D(1, 2);
        var b = new Point2D(3, 4);

        Assert.True(a.IsLessThanOrEqualTo(b));
    }

    [Fact]
    public void IsLessThanOrEqualTo_BothDimensionsEqual_ReturnsTrue()
    {
        var a = new Point2D(3, 4);
        var b = new Point2D(3, 4);

        Assert.True(a.IsLessThanOrEqualTo(b));
    }

    [Fact]
    public void IsLessThanOrEqualTo_OneDimensionGreater_ReturnsFalse()
    {
        var a = new Point2D(5, 2);
        var b = new Point2D(3, 4);

        Assert.False(a.IsLessThanOrEqualTo(b));
    }

    [Fact]
    public void IsGreaterThan_BothDimensionsGreater_ReturnsTrue()
    {
        var a = new Point2D(5, 6);
        var b = new Point2D(3, 4);

        Assert.True(a.IsGreaterThan(b));
    }

    [Fact]
    public void IsGreaterThan_OneDimensionEqual_ReturnsFalse()
    {
        var a = new Point2D(3, 6);
        var b = new Point2D(3, 4);

        Assert.False(a.IsGreaterThan(b));
    }

    [Fact]
    public void IsGreaterThan_OneDimensionLess_ReturnsFalse()
    {
        var a = new Point2D(5, 2);
        var b = new Point2D(3, 4);

        Assert.False(a.IsGreaterThan(b));
    }

    [Fact]
    public void IsGreaterThanOrEqualTo_BothDimensionsGreater_ReturnsTrue()
    {
        var a = new Point2D(5, 6);
        var b = new Point2D(3, 4);

        Assert.True(a.IsGreaterThanOrEqualTo(b));
    }

    [Fact]
    public void IsGreaterThanOrEqualTo_BothDimensionsEqual_ReturnsTrue()
    {
        var a = new Point2D(3, 4);
        var b = new Point2D(3, 4);

        Assert.True(a.IsGreaterThanOrEqualTo(b));
    }

    [Fact]
    public void IsGreaterThanOrEqualTo_OneDimensionLess_ReturnsFalse()
    {
        var a = new Point2D(5, 2);
        var b = new Point2D(3, 4);

        Assert.False(a.IsGreaterThanOrEqualTo(b));
    }

    [Fact]
    public void IsInBounds_PointInsideBounds_ReturnsTrue()
    {
        var point = new Point2D(5, 5);
        var size = new Size2D(10, 10);

        Assert.True(point.IsInBounds(size));
    }

    [Fact]
    public void IsInBounds_PointAtOrigin_ReturnsTrue()
    {
        var point = new Point2D(0, 0);
        var size = new Size2D(10, 10);

        Assert.True(point.IsInBounds(size));
    }

    [Fact]
    public void IsInBounds_PointAtMaxBoundary_ReturnsFalse()
    {
        // Bounds are [0, size), so size itself is out of bounds
        var point = new Point2D(10, 10);
        var size = new Size2D(10, 10);

        Assert.False(point.IsInBounds(size));
    }

    [Fact]
    public void IsInBounds_PointJustInsideMaxBoundary_ReturnsTrue()
    {
        var point = new Point2D(9, 9);
        var size = new Size2D(10, 10);

        Assert.True(point.IsInBounds(size));
    }

    [Fact]
    public void IsInBounds_NegativeX_ReturnsFalse()
    {
        var point = new Point2D(-1, 5);
        var size = new Size2D(10, 10);

        Assert.False(point.IsInBounds(size));
    }

    [Fact]
    public void IsInBounds_NegativeY_ReturnsFalse()
    {
        var point = new Point2D(5, -1);
        var size = new Size2D(10, 10);

        Assert.False(point.IsInBounds(size));
    }

    [Fact]
    public void IsInBounds_XOutOfBounds_ReturnsFalse()
    {
        var point = new Point2D(15, 5);
        var size = new Size2D(10, 10);

        Assert.False(point.IsInBounds(size));
    }

    [Fact]
    public void IsInBounds_YOutOfBounds_ReturnsFalse()
    {
        var point = new Point2D(5, 15);
        var size = new Size2D(10, 10);

        Assert.False(point.IsInBounds(size));
    }
}
