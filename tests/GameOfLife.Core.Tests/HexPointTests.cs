using Shouldly;

namespace GameOfLife.Core.Tests;

public class HexPointTests
{
    #region Equality Tests

    [Fact]
    public void Equality_SameCoordinates_AreEqual()
    {
        HexPoint point1 = (3, 5);
        HexPoint point2 = (3, 5);

        point1.ShouldBe(point2);
        (point1 == point2).ShouldBeTrue();
        point1.Equals(point2).ShouldBeTrue();
    }

    [Fact]
    public void Inequality_DifferentQCoordinate_AreNotEqual()
    {
        HexPoint point1 = (3, 5);
        HexPoint point2 = (4, 5);

        point1.ShouldNotBe(point2);
        (point1 != point2).ShouldBeTrue();
        point1.Equals(point2).ShouldBeFalse();
    }

    [Fact]
    public void Inequality_DifferentRCoordinate_AreNotEqual()
    {
        HexPoint point1 = (3, 5);
        HexPoint point2 = (3, 6);

        point1.ShouldNotBe(point2);
        (point1 != point2).ShouldBeTrue();
        point1.Equals(point2).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_EqualPoints_HaveSameHashCode()
    {
        HexPoint point1 = (3, 5);
        HexPoint point2 = (3, 5);

        point1.GetHashCode().ShouldBe(point2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentPoints_TypicallyHaveDifferentHashCodes()
    {
        HexPoint point1 = (3, 5);
        HexPoint point2 = (5, 3);

        point1.GetHashCode().ShouldNotBe(point2.GetHashCode());
    }

    #endregion

    #region Coordinate Tests

    [Fact]
    public void DefaultValue_HasZeroCoordinates()
    {
        var defaultPoint = default(HexPoint);

        defaultPoint.Q.ShouldBe(0);
        defaultPoint.R.ShouldBe(0);
    }

    [Fact]
    public void DerivedS_IsDerivedCorrectly()
    {
        // q + r + s = 0, so s = -q - r
        HexPoint point = (2, 3);

        point.S.ShouldBe(-5);
    }

    [Fact]
    public void DerivedS_AtOrigin_IsZero()
    {
        HexPoint point = default;

        point.S.ShouldBe(0);
    }

    [Fact]
    public void Constructor_NegativeCoordinates_CreatesValidPoint()
    {
        HexPoint point = (-5, -10);

        point.Q.ShouldBe(-5);
        point.R.ShouldBe(-10);
        point.S.ShouldBe(15); // -(-5) - (-10) = 5 + 10 = 15
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void Addition_TwoPoints_ReturnsSum()
    {
        HexPoint a = (3, 5);
        HexPoint b = (2, 7);

        HexPoint result = a + b;

        result.Q.ShouldBe(5);
        result.R.ShouldBe(12);
    }

    [Fact]
    public void Addition_WithNegativeCoordinates_ReturnsCorrectSum()
    {
        HexPoint a = (-3, 5);
        HexPoint b = (2, -7);

        HexPoint result = a + b;

        result.Q.ShouldBe(-1);
        result.R.ShouldBe(-2);
    }

    [Fact]
    public void Subtraction_TwoPoints_ReturnsDifference()
    {
        HexPoint a = (5, 10);
        HexPoint b = (2, 3);

        HexPoint result = a - b;

        result.Q.ShouldBe(3);
        result.R.ShouldBe(7);
    }

    [Fact]
    public void Subtraction_ResultsInNegative_ReturnsCorrectDifference()
    {
        HexPoint a = (2, 3);
        HexPoint b = (5, 10);

        HexPoint result = a - b;

        result.Q.ShouldBe(-3);
        result.R.ShouldBe(-7);
    }

    #endregion

    #region Tuple Conversion Tests

    [Fact]
    public void ImplicitConversion_FromTuple_CreatesPoint()
    {
        HexPoint point = (5, 10);

        point.Q.ShouldBe(5);
        point.R.ShouldBe(10);
    }

    [Fact]
    public void ImplicitConversion_InCollection_WorksCorrectly()
    {
        HexPoint[] points = [default, (1, 2), (3, 4)];

        points.Length.ShouldBe(3);
        points[0].ShouldBe(default);
        points[1].ShouldBe((1, 2));
        points[2].ShouldBe((3, 4));
    }

    #endregion

    #region Distance Tests

    [Fact]
    public void DistanceTo_SamePoint_IsZero()
    {
        HexPoint point = (3, 5);

        point.DistanceTo(point).ShouldBe(0);
    }

    [Fact]
    public void DistanceTo_AdjacentPoint_IsOne()
    {
        HexPoint point1 = default;
        HexPoint point2 = (1, 0); // East neighbor

        point1.DistanceTo(point2).ShouldBe(1);
    }

    [Fact]
    public void DistanceTo_AllSixNeighbors_AreDistanceOne()
    {
        HexPoint center = default;
        HexPoint[] neighbors =
        [
            (1, 0),   // East
            (-1, 0),  // West
            (1, -1),  // Northeast
            (0, -1),  // Northwest
            (0, 1),   // Southeast
            (-1, 1)   // Southwest
        ];

        foreach (HexPoint neighbor in neighbors)
        {
            center.DistanceTo(neighbor).ShouldBe(1);
        }
    }

    [Fact]
    public void DistanceTo_TwoStepsAway_IsTwo()
    {
        HexPoint point1 = default;
        HexPoint point2 = (2, 0); // Two steps east

        point1.DistanceTo(point2).ShouldBe(2);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        HexPoint a = (3, 2);
        HexPoint b = (-1, 4);

        a.DistanceTo(b).ShouldBe(b.DistanceTo(a));
    }

    #endregion

    #region IsWithinRadius Tests

    [Fact]
    public void IsWithinRadius_Origin_AlwaysWithin()
    {
        HexPoint origin = default;

        origin.IsWithinRadius(0).ShouldBeTrue();
        origin.IsWithinRadius(1).ShouldBeTrue();
        origin.IsWithinRadius(10).ShouldBeTrue();
    }

    [Fact]
    public void IsWithinRadius_NeighborWithRadiusZero_IsFalse()
    {
        HexPoint neighbor = (1, 0);

        neighbor.IsWithinRadius(0).ShouldBeFalse();
    }

    [Fact]
    public void IsWithinRadius_NeighborWithRadiusOne_IsTrue()
    {
        HexPoint neighbor = (1, 0);

        neighbor.IsWithinRadius(1).ShouldBeTrue();
    }

    [Fact]
    public void IsWithinRadius_AllSixNeighborsWithRadiusOne_AreWithin()
    {
        HexPoint[] neighbors =
        [
            (1, 0),
            (-1, 0),
            (1, -1),
            (0, -1),
            (0, 1),
            (-1, 1)
        ];

        foreach (HexPoint neighbor in neighbors)
        {
            neighbor.IsWithinRadius(1).ShouldBeTrue();
        }
    }

    [Fact]
    public void IsWithinRadius_PointOutsideRadius_IsFalse()
    {
        HexPoint farPoint = (5, 5);

        farPoint.IsWithinRadius(3).ShouldBeFalse();
    }

    [Fact]
    public void IsWithinRadius_PointAtExactRadius_IsTrue()
    {
        HexPoint point = (3, 0);

        point.IsWithinRadius(3).ShouldBeTrue();
    }

    #endregion

    #region IEquatable Tests

    [Fact]
    public void IEquatable_IsImplemented()
    {
        HexPoint point = (3, 5);

        _ = point.ShouldBeAssignableTo<IEquatable<HexPoint>>();
    }

    [Fact]
    public void HashSet_CanStoreAndRetrievePoints()
    {
        var set = new HashSet<HexPoint>
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
        var dict = new Dictionary<HexPoint, string>
        {
            [default] = "origin",
            [(1, 0)] = "east"
        };

        dict[default].ShouldBe("origin");
        dict[(1, 0)].ShouldBe("east");
    }

    #endregion
}
