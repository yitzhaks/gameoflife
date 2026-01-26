namespace GameOfLife.Core.Tests;

public class Point2DTests
{
    [Fact]
    public void Equality_SameCoordinates_AreEqual()
    {
        var point1 = new Point2D(3, 5);
        var point2 = new Point2D(3, 5);

        Assert.Equal(point1, point2);
        Assert.True(point1 == point2);
        Assert.True(point1.Equals(point2));
    }

    [Fact]
    public void Inequality_DifferentXCoordinate_AreNotEqual()
    {
        var point1 = new Point2D(3, 5);
        var point2 = new Point2D(4, 5);

        Assert.NotEqual(point1, point2);
        Assert.True(point1 != point2);
        Assert.False(point1.Equals(point2));
    }

    [Fact]
    public void Inequality_DifferentYCoordinate_AreNotEqual()
    {
        var point1 = new Point2D(3, 5);
        var point2 = new Point2D(3, 6);

        Assert.NotEqual(point1, point2);
        Assert.True(point1 != point2);
        Assert.False(point1.Equals(point2));
    }

    [Fact]
    public void Inequality_BothCoordinatesDifferent_AreNotEqual()
    {
        var point1 = new Point2D(3, 5);
        var point2 = new Point2D(7, 9);

        Assert.NotEqual(point1, point2);
        Assert.True(point1 != point2);
        Assert.False(point1.Equals(point2));
    }

    [Fact]
    public void GetHashCode_EqualPoints_HaveSameHashCode()
    {
        var point1 = new Point2D(3, 5);
        var point2 = new Point2D(3, 5);

        Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentPoints_TypicallyHaveDifferentHashCodes()
    {
        var point1 = new Point2D(3, 5);
        var point2 = new Point2D(5, 3);

        // Hash codes are not guaranteed to be different, but for typical implementations
        // swapping coordinates should produce different hashes
        Assert.NotEqual(point1.GetHashCode(), point2.GetHashCode());
    }

    [Fact]
    public void DefaultValue_HasZeroCoordinates()
    {
        var defaultPoint = default(Point2D);

        Assert.Equal(0, defaultPoint.X);
        Assert.Equal(0, defaultPoint.Y);
    }

    [Fact]
    public void DefaultValue_EqualsExplicitZeroPoint()
    {
        var defaultPoint = default(Point2D);
        var zeroPoint = new Point2D(0, 0);

        Assert.Equal(defaultPoint, zeroPoint);
    }

    [Fact]
    public void Constructor_NegativeCoordinates_CreatesValidPoint()
    {
        var point = new Point2D(-5, -10);

        Assert.Equal(-5, point.X);
        Assert.Equal(-10, point.Y);
    }

    [Fact]
    public void Equality_WithObjectEquals_WorksCorrectly()
    {
        var point1 = new Point2D(3, 5);
        object point2 = new Point2D(3, 5);
        object point3 = new Point2D(4, 5);

        Assert.True(point1.Equals(point2));
        Assert.False(point1.Equals(point3));
    }

    [Fact]
    public void Equality_WithNull_ReturnsFalse()
    {
        var point = new Point2D(3, 5);

        Assert.False(point.Equals(null));
    }

    [Fact]
    public void Equality_WithDifferentType_ReturnsFalse()
    {
        var point = new Point2D(3, 5);

        Assert.False(point.Equals("not a point"));
        Assert.False(point.Equals(42));
    }

    [Fact]
    public void IEquatable_IsImplemented()
    {
        var point = new Point2D(3, 5);

        Assert.IsAssignableFrom<IEquatable<Point2D>>(point);
    }

    [Fact]
    public void HashSet_CanStoreAndRetrievePoints()
    {
        var set = new HashSet<Point2D>
        {
            (0, 0),
            (1, 1),
            (2, 2)
        };

        Assert.Contains((1, 1), set);
        Assert.DoesNotContain((3, 3), set);
        Assert.Equal(3, set.Count);
    }

    [Fact]
    public void Dictionary_CanUsePointAsKey()
    {
        var dict = new Dictionary<Point2D, string>
        {
            [(0, 0)] = "origin",
            [(1, 0)] = "right"
        };

        Assert.Equal("origin", dict[(0, 0)]);
        Assert.Equal("right", dict[(1, 0)]);
    }

    [Fact]
    public void ImplicitConversion_FromTuple_CreatesPoint()
    {
        Point2D point = (5, 10);

        Assert.Equal(5, point.X);
        Assert.Equal(10, point.Y);
    }

    [Fact]
    public void ImplicitConversion_InCollection_WorksCorrectly()
    {
        Point2D[] points = [(0, 0), (1, 2), (3, 4)];

        Assert.Equal(3, points.Length);
        Assert.Equal(new Point2D(0, 0), points[0]);
        Assert.Equal(new Point2D(1, 2), points[1]);
        Assert.Equal(new Point2D(3, 4), points[2]);
    }
}
