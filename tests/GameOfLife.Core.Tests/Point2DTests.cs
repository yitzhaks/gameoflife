using Shouldly;

namespace GameOfLife.Core.Tests;

public class Point2DTests
{
    [Fact]
    public void Equality_SameCoordinates_AreEqual()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (3, 5);

        point1.ShouldBe(point2);
        (point1 == point2).ShouldBeTrue();
        point1.Equals(point2).ShouldBeTrue();
    }

    [Fact]
    public void Inequality_DifferentXCoordinate_AreNotEqual()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (4, 5);

        point1.ShouldNotBe(point2);
        (point1 != point2).ShouldBeTrue();
        point1.Equals(point2).ShouldBeFalse();
    }

    [Fact]
    public void Inequality_DifferentYCoordinate_AreNotEqual()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (3, 6);

        point1.ShouldNotBe(point2);
        (point1 != point2).ShouldBeTrue();
        point1.Equals(point2).ShouldBeFalse();
    }

    [Fact]
    public void Inequality_BothCoordinatesDifferent_AreNotEqual()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (7, 9);

        point1.ShouldNotBe(point2);
        (point1 != point2).ShouldBeTrue();
        point1.Equals(point2).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_EqualPoints_HaveSameHashCode()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (3, 5);

        point1.GetHashCode().ShouldBe(point2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentPoints_TypicallyHaveDifferentHashCodes()
    {
        Point2D point1 = (3, 5);
        Point2D point2 = (5, 3);

        // Hash codes are not guaranteed to be different, but for typical implementations
        // swapping coordinates should produce different hashes
        point1.GetHashCode().ShouldNotBe(point2.GetHashCode());
    }

    [Fact]
    public void DefaultValue_HasZeroCoordinates()
    {
        var defaultPoint = default(Point2D);

        defaultPoint.X.ShouldBe(0);
        defaultPoint.Y.ShouldBe(0);
    }

    [Fact]
    public void DefaultValue_EqualsExplicitZeroPoint()
    {
        var defaultPoint = default(Point2D);
        Point2D zeroPoint = default;

        defaultPoint.ShouldBe(zeroPoint);
    }

    [Fact]
    public void Constructor_NegativeCoordinates_CreatesValidPoint()
    {
        Point2D point = (-5, -10);

        point.X.ShouldBe(-5);
        point.Y.ShouldBe(-10);
    }

    [Fact]
    public void Equality_WithObjectEquals_WorksCorrectly()
    {
        Point2D point1 = (3, 5);
        object point2 = (Point2D)(3, 5);
        object point3 = (Point2D)(4, 5);

        point1.Equals(point2).ShouldBeTrue();
        point1.Equals(point3).ShouldBeFalse();
    }

    [Fact]
    public void Equality_WithNull_ReturnsFalse()
    {
        Point2D point = (3, 5);

        point.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Equality_WithDifferentType_ReturnsFalse()
    {
        Point2D point = (3, 5);

        point.Equals("not a point").ShouldBeFalse();
        point.Equals(42).ShouldBeFalse();
    }

    [Fact]
    public void IEquatable_IsImplemented()
    {
        Point2D point = (3, 5);

        _ = point.ShouldBeAssignableTo<IEquatable<Point2D>>();
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

        set.ShouldContain((1, 1));
        set.ShouldNotContain((3, 3));
        set.Count.ShouldBe(3);
    }

    [Fact]
    public void Dictionary_CanUsePointAsKey()
    {
        var dict = new Dictionary<Point2D, string>
        {
            [default] = "origin",
            [(1, 0)] = "right"
        };

        dict[default].ShouldBe("origin");
        dict[(1, 0)].ShouldBe("right");
    }

    [Fact]
    public void ImplicitConversion_FromTuple_CreatesPoint()
    {
        Point2D point = (5, 10);

        point.X.ShouldBe(5);
        point.Y.ShouldBe(10);
    }

    [Fact]
    public void ImplicitConversion_InCollection_WorksCorrectly()
    {
        Point2D[] points = [default, (1, 2), (3, 4)];

        points.Length.ShouldBe(3);
        points[0].ShouldBe(default);
        points[1].ShouldBe((1, 2));
        points[2].ShouldBe((3, 4));
    }

    [Fact]
    public void Addition_TwoPoints_ReturnsSum()
    {
        Point2D a = (3, 5);
        Point2D b = (2, 7);

        Point2D result = a + b;

        result.X.ShouldBe(5);
        result.Y.ShouldBe(12);
    }

    [Fact]
    public void Addition_WithNegativeCoordinates_ReturnsCorrectSum()
    {
        Point2D a = (-3, 5);
        Point2D b = (2, -7);

        Point2D result = a + b;

        result.X.ShouldBe(-1);
        result.Y.ShouldBe(-2);
    }

    [Fact]
    public void Subtraction_TwoPoints_ReturnsDifference()
    {
        Point2D a = (5, 10);
        Point2D b = (2, 3);

        Point2D result = a - b;

        result.X.ShouldBe(3);
        result.Y.ShouldBe(7);
    }

    [Fact]
    public void Subtraction_ResultsInNegative_ReturnsCorrectDifference()
    {
        Point2D a = (2, 3);
        Point2D b = (5, 10);

        Point2D result = a - b;

        result.X.ShouldBe(-3);
        result.Y.ShouldBe(-7);
    }
}
