namespace GameOfLife.Core.Tests;

public class RectangularTopologyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidDimensions_CreatesGrid()
    {
        var topology = new RectangularTopology((5, 10));

        Assert.NotNull(topology);
        Assert.Equal(50, topology.Nodes.Count());
    }

    [Fact]
    public void Constructor_WidthLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => new RectangularTopology((-1, 5)));

        Assert.Equal("size", exception.ParamName);
    }

    [Fact]
    public void Constructor_WidthEqualsZero_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => new RectangularTopology((0, 5)));

        Assert.Equal("size", exception.ParamName);
    }

    [Fact]
    public void Constructor_HeightLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => new RectangularTopology((5, -1)));

        Assert.Equal("size", exception.ParamName);
    }

    [Fact]
    public void Constructor_HeightEqualsZero_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => new RectangularTopology((5, 0)));

        Assert.Equal("size", exception.ParamName);
    }

    #endregion

    #region Nodes Property Tests

    [Fact]
    public void Nodes_Count_EqualsWidthTimesHeight()
    {
        var topology = new RectangularTopology((4, 3));

        Assert.Equal(12, topology.Nodes.Count());
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

        Assert.Equal(expectedPoints.Length, nodes.Count);
        foreach (Point2D expected in expectedPoints)
        {
            Assert.Contains(expected, nodes);
        }
    }

    [Fact]
    public void Nodes_1x1Grid_HasExactlyOneNode()
    {
        var topology = new RectangularTopology((1, 1));
        var nodes = topology.Nodes.ToList();

        _ = Assert.Single(nodes);
        Assert.Equal(default, nodes[0]);
    }

    #endregion

    #region GetNeighbors Tests - Valid Nodes

    [Fact]
    public void GetNeighbors_CornerCell00_HasExactly3Neighbors()
    {
        var topology = new RectangularTopology((5, 5));
        var neighbors = topology.GetNeighbors(default).ToList();

        Assert.Equal(3, neighbors.Count);
        Assert.Contains((1, 0), neighbors);
        Assert.Contains((0, 1), neighbors);
        Assert.Contains((1, 1), neighbors);
    }

    [Fact]
    public void GetNeighbors_CornerCellBottomRight_HasExactly3Neighbors()
    {
        var topology = new RectangularTopology((5, 5));
        var neighbors = topology.GetNeighbors((4, 4)).ToList();

        Assert.Equal(3, neighbors.Count);
        Assert.Contains((3, 3), neighbors);
        Assert.Contains((4, 3), neighbors);
        Assert.Contains((3, 4), neighbors);
    }

    [Fact]
    public void GetNeighbors_EdgeCell_HasExactly5Neighbors()
    {
        var topology = new RectangularTopology((5, 5));
        // Testing edge cell at (2, 0) - top edge, not a corner
        var neighbors = topology.GetNeighbors((2, 0)).ToList();

        Assert.Equal(5, neighbors.Count);
        Assert.Contains((1, 0), neighbors);
        Assert.Contains((3, 0), neighbors);
        Assert.Contains((1, 1), neighbors);
        Assert.Contains((2, 1), neighbors);
        Assert.Contains((3, 1), neighbors);
    }

    [Fact]
    public void GetNeighbors_InteriorCell_HasExactly8Neighbors()
    {
        var topology = new RectangularTopology((5, 5));
        var neighbors = topology.GetNeighbors((2, 2)).ToList();

        Assert.Equal(8, neighbors.Count);
        Assert.Contains((1, 1), neighbors);
        Assert.Contains((2, 1), neighbors);
        Assert.Contains((3, 1), neighbors);
        Assert.Contains((1, 2), neighbors);
        Assert.Contains((3, 2), neighbors);
        Assert.Contains((1, 3), neighbors);
        Assert.Contains((2, 3), neighbors);
        Assert.Contains((3, 3), neighbors);
    }

    #endregion

    #region GetNeighbors Tests - Invalid Nodes

    [Fact]
    public void GetNeighbors_NegativeX_ThrowsArgumentOutOfRangeException()
    {
        var topology = new RectangularTopology((5, 5));

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((-1, 2)).ToList());

        Assert.Equal("node", exception.ParamName);
    }

    [Fact]
    public void GetNeighbors_XGreaterThanOrEqualToWidth_ThrowsArgumentOutOfRangeException()
    {
        var topology = new RectangularTopology((5, 5));

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((5, 2)).ToList());

        Assert.Equal("node", exception.ParamName);
    }

    [Fact]
    public void GetNeighbors_NegativeY_ThrowsArgumentOutOfRangeException()
    {
        var topology = new RectangularTopology((5, 5));

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((2, -1)).ToList());

        Assert.Equal("node", exception.ParamName);
    }

    [Fact]
    public void GetNeighbors_YGreaterThanOrEqualToHeight_ThrowsArgumentOutOfRangeException()
    {
        var topology = new RectangularTopology((5, 5));

        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((2, 5)).ToList());

        Assert.Equal("node", exception.ParamName);
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
            Assert.Contains(centerNode, neighborOfNeighbor);
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
            Assert.Contains(edgeNode, neighborOfNeighbor);
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
                Assert.Contains(corner, neighborOfNeighbor);
            }
        }
    }

    #endregion

    #region ITopology Interface Tests

    [Fact]
    public void RectangularTopology_ImplementsITopology()
    {
        var topology = new RectangularTopology((3, 3));

        _ = Assert.IsAssignableFrom<ITopology<Point2D>>(topology);
    }

    #endregion
}
