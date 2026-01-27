using Shouldly;

namespace GameOfLife.Core.Tests;

public class RectangularTopologyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidDimensions_CreatesGrid()
    {
        var topology = new RectangularTopology((5, 10));

        _ = topology.ShouldNotBeNull();
        topology.Nodes.Count().ShouldBe(50);
    }

    [Fact]
    public void Constructor_WidthLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() => new RectangularTopology((-1, 5)));

        exception.ParamName.ShouldBe("size");
    }

    [Fact]
    public void Constructor_WidthEqualsZero_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() => new RectangularTopology((0, 5)));

        exception.ParamName.ShouldBe("size");
    }

    [Fact]
    public void Constructor_HeightLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() => new RectangularTopology((5, -1)));

        exception.ParamName.ShouldBe("size");
    }

    [Fact]
    public void Constructor_HeightEqualsZero_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() => new RectangularTopology((5, 0)));

        exception.ParamName.ShouldBe("size");
    }

    #endregion

    #region Nodes Property Tests

    [Fact]
    public void Nodes_Count_EqualsWidthTimesHeight()
    {
        var topology = new RectangularTopology((4, 3));

        topology.Nodes.Count().ShouldBe(12);
    }

    [Fact]
    public void Nodes_ContainsAllExpectedPoints_FromOriginToMaxBounds()
    {
        var topology = new RectangularTopology((3, 2));
        var nodes = topology.Nodes.ToList();

        Point2D[] expectedPoints =
        [
            default, (1, 0), (2, 0),
            (0, 1), (1, 1), (2, 1)
        ];

        nodes.Count.ShouldBe(expectedPoints.Length);
        foreach (Point2D expected in expectedPoints)
        {
            nodes.ShouldContain(expected);
        }
    }

    [Fact]
    public void Nodes_1x1Grid_HasExactlyOneNode()
    {
        var topology = new RectangularTopology((1, 1));
        var nodes = topology.Nodes.ToList();

        _ = nodes.ShouldHaveSingleItem();
        nodes[0].ShouldBe(default);
    }

    #endregion

    #region GetNeighbors Tests - Valid Nodes

    [Fact]
    public void GetNeighbors_CornerCell00_HasExactly3Neighbors()
    {
        var topology = new RectangularTopology((5, 5));
        var neighbors = topology.GetNeighbors(default).ToList();

        neighbors.Count.ShouldBe(3);
        neighbors.ShouldContain((1, 0));
        neighbors.ShouldContain((0, 1));
        neighbors.ShouldContain((1, 1));
    }

    [Fact]
    public void GetNeighbors_CornerCellBottomRight_HasExactly3Neighbors()
    {
        var topology = new RectangularTopology((5, 5));
        var neighbors = topology.GetNeighbors((4, 4)).ToList();

        neighbors.Count.ShouldBe(3);
        neighbors.ShouldContain((3, 3));
        neighbors.ShouldContain((4, 3));
        neighbors.ShouldContain((3, 4));
    }

    [Fact]
    public void GetNeighbors_EdgeCell_HasExactly5Neighbors()
    {
        var topology = new RectangularTopology((5, 5));
        // Testing edge cell at (2, 0) - top edge, not a corner
        var neighbors = topology.GetNeighbors((2, 0)).ToList();

        neighbors.Count.ShouldBe(5);
        neighbors.ShouldContain((1, 0));
        neighbors.ShouldContain((3, 0));
        neighbors.ShouldContain((1, 1));
        neighbors.ShouldContain((2, 1));
        neighbors.ShouldContain((3, 1));
    }

    [Fact]
    public void GetNeighbors_InteriorCell_HasExactly8Neighbors()
    {
        var topology = new RectangularTopology((5, 5));
        var neighbors = topology.GetNeighbors((2, 2)).ToList();

        neighbors.Count.ShouldBe(8);
        neighbors.ShouldContain((1, 1));
        neighbors.ShouldContain((2, 1));
        neighbors.ShouldContain((3, 1));
        neighbors.ShouldContain((1, 2));
        neighbors.ShouldContain((3, 2));
        neighbors.ShouldContain((1, 3));
        neighbors.ShouldContain((2, 3));
        neighbors.ShouldContain((3, 3));
    }

    #endregion

    #region GetNeighbors Tests - Invalid Nodes

    [Fact]
    public void GetNeighbors_NegativeX_ThrowsArgumentOutOfRangeException()
    {
        var topology = new RectangularTopology((5, 5));

        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((-1, 2)).ToList());

        exception.ParamName.ShouldBe("node");
    }

    [Fact]
    public void GetNeighbors_XGreaterThanOrEqualToWidth_ThrowsArgumentOutOfRangeException()
    {
        var topology = new RectangularTopology((5, 5));

        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((5, 2)).ToList());

        exception.ParamName.ShouldBe("node");
    }

    [Fact]
    public void GetNeighbors_NegativeY_ThrowsArgumentOutOfRangeException()
    {
        var topology = new RectangularTopology((5, 5));

        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((2, -1)).ToList());

        exception.ParamName.ShouldBe("node");
    }

    [Fact]
    public void GetNeighbors_YGreaterThanOrEqualToHeight_ThrowsArgumentOutOfRangeException()
    {
        var topology = new RectangularTopology((5, 5));

        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((2, 5)).ToList());

        exception.ParamName.ShouldBe("node");
    }

    #endregion

    #region GetNeighbors Tests - Symmetry

    [Fact]
    public void GetNeighbors_Symmetry_IfAIsNeighborOfB_ThenBIsNeighborOfA()
    {
        var topology = new RectangularTopology((5, 5));

        // Test with an interior cell and verify symmetry with all its neighbors
        Point2D centerNode = (2, 2);
        var centerNeighbors = topology.GetNeighbors(centerNode).ToList();

        foreach (Point2D neighbor in centerNeighbors)
        {
            var neighborOfNeighbor = topology.GetNeighbors(neighbor).ToList();
            neighborOfNeighbor.ShouldContain(centerNode);
        }
    }

    [Fact]
    public void GetNeighbors_Symmetry_AllEdgeCells_MaintainSymmetry()
    {
        var topology = new RectangularTopology((4, 4));

        // Test edge cell at (0, 1)
        Point2D edgeNode = (0, 1);
        var edgeNeighbors = topology.GetNeighbors(edgeNode).ToList();

        foreach (Point2D neighbor in edgeNeighbors)
        {
            var neighborOfNeighbor = topology.GetNeighbors(neighbor).ToList();
            neighborOfNeighbor.ShouldContain(edgeNode);
        }
    }

    [Fact]
    public void GetNeighbors_Symmetry_AllCornerCells_MaintainSymmetry()
    {
        var topology = new RectangularTopology((3, 3));

        Point2D[] corners =
        [
            default,
            (2, 0),
            (0, 2),
            (2, 2)
        ];

        foreach (Point2D corner in corners)
        {
            var cornerNeighbors = topology.GetNeighbors(corner).ToList();

            foreach (Point2D neighbor in cornerNeighbors)
            {
                var neighborOfNeighbor = topology.GetNeighbors(neighbor).ToList();
                neighborOfNeighbor.ShouldContain(corner);
            }
        }
    }

    #endregion

    #region ITopology Interface Tests

    [Fact]
    public void RectangularTopology_ImplementsITopology()
    {
        var topology = new RectangularTopology((3, 3));

        _ = topology.ShouldBeAssignableTo<ITopology<Point2D>>();
    }

    #endregion
}
