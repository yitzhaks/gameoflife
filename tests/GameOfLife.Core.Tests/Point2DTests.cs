namespace GameOfLife.Core.Tests;

public class Point2DTests
{
    [Fact]
    public void Equality_SameCoordinates_AreEqual()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (3, 5);

        Assert.Equal(point1, point2);
        Assert.True(point1 == point2);
        Assert.True(point1.Equals(point2));
    }

    [Fact]
    public void Inequality_DifferentXCoordinate_AreNotEqual()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (4, 5);

        Assert.NotEqual(point1, point2);
        Assert.True(point1 != point2);
        Assert.False(point1.Equals(point2));
    }

    [Fact]
    public void Inequality_DifferentYCoordinate_AreNotEqual()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (3, 6);

        Assert.NotEqual(point1, point2);
        Assert.True(point1 != point2);
        Assert.False(point1.Equals(point2));
    }

    [Fact]
    public void Inequality_BothCoordinatesDifferent_AreNotEqual()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (7, 9);

        Assert.NotEqual(point1, point2);
        Assert.True(point1 != point2);
        Assert.False(point1.Equals(point2));
    }

    [Fact]
    public void GetHashCode_EqualPoints_HaveSameHashCode()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (3, 5);

        Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentPoints_TypicallyHaveDifferentHashCodes()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (5, 3);

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
        Point2D zeroPoint = default;

        Assert.Equal(defaultPoint, zeroPoint);
    }

    [Fact]
    public void Constructor_NegativeCoordinates_CreatesValidPoint()
    {
        Point2D point = (-5, -10);

        Assert.Equal(-5, point.X);
        Assert.Equal(-10, point.Y);
    }

    [Fact]
    public void Equality_WithObjectEquals_WorksCorrectly()
    {
        Point2D point1 = (3, 5);
        object point2 = (Point2D)(3, 5);
        object point3 = (Point2D)(4, 5);

        Assert.True(point1.Equals(point2));
        Assert.False(point1.Equals(point3));
    }

    [Fact]
    public void Equality_WithNull_ReturnsFalse()
    {
        Point2D point = (3, 5);

        Assert.False(point.Equals(null));
    }

    [Fact]
    public void Equality_WithDifferentType_ReturnsFalse()
    {
        Point2D point = (3, 5);

        Assert.False(point.Equals("not a point"));
        Assert.False(point.Equals(42));
    }

    [Fact]
    public void IEquatable_IsImplemented()
    {
        Point2D point = (3, 5);

        _ = Assert.IsAssignableFrom<IEquatable<Point2D>>(point);
    }

    [Fact]
    public void HashSet_CanStoreAndRetrievePoints()
    {
        var set = new HashSet<Point2D>
        {
            default,
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
            [default] = "origin",
            [(1, 0)] = "right"
        };

        Assert.Equal("origin", dict[default]);
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
        Point2D[] points = [default, (1, 2), (3, 4)];

        Assert.Equal(3, points.Length);
        Assert.Equal(default, points[0]);
        Assert.Equal((1, 2), points[1]);
        Assert.Equal((3, 4), points[2]);
    }

    [Fact]
    public void Addition_TwoPoints_ReturnsSum()
    {
        Point2D a = (3, 5);
        Point2D b = (2, 7);

        Point2D result = a + b;

        Assert.Equal(5, result.X);
        Assert.Equal(12, result.Y);
    }

    [Fact]
    public void Addition_WithNegativeCoordinates_ReturnsCorrectSum()
    {
        Point2D a = (-3, 5);
        Point2D b = (2, -7);

        Point2D result = a + b;

        Assert.Equal(-1, result.X);
        Assert.Equal(-2, result.Y);
    }

    [Fact]
    public void Subtraction_TwoPoints_ReturnsDifference()
    {
        Point2D a = (5, 10);
        Point2D b = (2, 3);

        Point2D result = a - b;

        Assert.Equal(3, result.X);
        Assert.Equal(7, result.Y);
    }

    [Fact]
    public void Subtraction_ResultsInNegative_ReturnsCorrectDifference()
    {
        Point2D a = (2, 3);
        Point2D b = (5, 10);

        Point2D result = a - b;

        Assert.Equal(-3, result.X);
        Assert.Equal(-7, result.Y);
    }
}
