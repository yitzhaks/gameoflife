namespace GameOfLife.Core.Tests;

public class Grid2DTopologyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ValidDimensions_CreatesGrid()
    {
        var topology = new Grid2DTopology(5, 10);

        Assert.NotNull(topology);
        Assert.Equal(50, topology.Nodes.Count());
    }

    [Fact]
    public void Constructor_WidthLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Grid2DTopology(-1, 5));

        Assert.Equal("width", exception.ParamName);
    }

    [Fact]
    public void Constructor_WidthEqualsZero_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Grid2DTopology(0, 5));

        Assert.Equal("width", exception.ParamName);
    }

    [Fact]
    public void Constructor_HeightLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Grid2DTopology(5, -1));

        Assert.Equal("height", exception.ParamName);
    }

    [Fact]
    public void Constructor_HeightEqualsZero_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Grid2DTopology(5, 0));

        Assert.Equal("height", exception.ParamName);
    }

    #endregion

    #region Nodes Property Tests

    [Fact]
    public void Nodes_Count_EqualsWidthTimesHeight()
    {
        var topology = new Grid2DTopology(4, 3);

        Assert.Equal(12, topology.Nodes.Count());
    }

    [Fact]
    public void Nodes_ContainsAllExpectedPoints_FromOriginToMaxBounds()
    {
        var topology = new Grid2DTopology(3, 2);
        var nodes = topology.Nodes.ToList();

        var expectedPoints = new[]
        {
            new Point2D(0, 0), new Point2D(1, 0), new Point2D(2, 0),
            new Point2D(0, 1), new Point2D(1, 1), new Point2D(2, 1)
        };

        Assert.Equal(expectedPoints.Length, nodes.Count);
        foreach (var expected in expectedPoints)
        {
            Assert.Contains(expected, nodes);
        }
    }

    [Fact]
    public void Nodes_1x1Grid_HasExactlyOneNode()
    {
        var topology = new Grid2DTopology(1, 1);
        var nodes = topology.Nodes.ToList();

        Assert.Single(nodes);
        Assert.Equal(new Point2D(0, 0), nodes[0]);
    }

    #endregion

    #region GetNeighbors Tests - Valid Nodes

    [Fact]
    public void GetNeighbors_CornerCell00_HasExactly3Neighbors()
    {
        var topology = new Grid2DTopology(5, 5);
        var neighbors = topology.GetNeighbors(new Point2D(0, 0)).ToList();

        Assert.Equal(3, neighbors.Count);
        Assert.Contains(new Point2D(1, 0), neighbors);
        Assert.Contains(new Point2D(0, 1), neighbors);
        Assert.Contains(new Point2D(1, 1), neighbors);
    }

    [Fact]
    public void GetNeighbors_CornerCellBottomRight_HasExactly3Neighbors()
    {
        var topology = new Grid2DTopology(5, 5);
        var neighbors = topology.GetNeighbors(new Point2D(4, 4)).ToList();

        Assert.Equal(3, neighbors.Count);
        Assert.Contains(new Point2D(3, 3), neighbors);
        Assert.Contains(new Point2D(4, 3), neighbors);
        Assert.Contains(new Point2D(3, 4), neighbors);
    }

    [Fact]
    public void GetNeighbors_EdgeCell_HasExactly5Neighbors()
    {
        var topology = new Grid2DTopology(5, 5);
        // Testing edge cell at (2, 0) - top edge, not a corner
        var neighbors = topology.GetNeighbors(new Point2D(2, 0)).ToList();

        Assert.Equal(5, neighbors.Count);
        Assert.Contains(new Point2D(1, 0), neighbors);
        Assert.Contains(new Point2D(3, 0), neighbors);
        Assert.Contains(new Point2D(1, 1), neighbors);
        Assert.Contains(new Point2D(2, 1), neighbors);
        Assert.Contains(new Point2D(3, 1), neighbors);
    }

    [Fact]
    public void GetNeighbors_InteriorCell_HasExactly8Neighbors()
    {
        var topology = new Grid2DTopology(5, 5);
        var neighbors = topology.GetNeighbors(new Point2D(2, 2)).ToList();

        Assert.Equal(8, neighbors.Count);
        Assert.Contains(new Point2D(1, 1), neighbors);
        Assert.Contains(new Point2D(2, 1), neighbors);
        Assert.Contains(new Point2D(3, 1), neighbors);
        Assert.Contains(new Point2D(1, 2), neighbors);
        Assert.Contains(new Point2D(3, 2), neighbors);
        Assert.Contains(new Point2D(1, 3), neighbors);
        Assert.Contains(new Point2D(2, 3), neighbors);
        Assert.Contains(new Point2D(3, 3), neighbors);
    }

    #endregion

    #region GetNeighbors Tests - Invalid Nodes

    [Fact]
    public void GetNeighbors_NegativeX_ThrowsArgumentOutOfRangeException()
    {
        var topology = new Grid2DTopology(5, 5);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors(new Point2D(-1, 2)).ToList());

        Assert.Equal("node", exception.ParamName);
    }

    [Fact]
    public void GetNeighbors_XGreaterThanOrEqualToWidth_ThrowsArgumentOutOfRangeException()
    {
        var topology = new Grid2DTopology(5, 5);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors(new Point2D(5, 2)).ToList());

        Assert.Equal("node", exception.ParamName);
    }

    [Fact]
    public void GetNeighbors_NegativeY_ThrowsArgumentOutOfRangeException()
    {
        var topology = new Grid2DTopology(5, 5);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors(new Point2D(2, -1)).ToList());

        Assert.Equal("node", exception.ParamName);
    }

    [Fact]
    public void GetNeighbors_YGreaterThanOrEqualToHeight_ThrowsArgumentOutOfRangeException()
    {
        var topology = new Grid2DTopology(5, 5);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors(new Point2D(2, 5)).ToList());

        Assert.Equal("node", exception.ParamName);
    }

    #endregion

    #region GetNeighbors Tests - Symmetry

    [Fact]
    public void GetNeighbors_Symmetry_IfAIsNeighborOfB_ThenBIsNeighborOfA()
    {
        var topology = new Grid2DTopology(5, 5);

        // Test with an interior cell and verify symmetry with all its neighbors
        var centerNode = new Point2D(2, 2);
        var centerNeighbors = topology.GetNeighbors(centerNode).ToList();

        foreach (var neighbor in centerNeighbors)
        {
            var neighborOfNeighbor = topology.GetNeighbors(neighbor).ToList();
            Assert.Contains(centerNode, neighborOfNeighbor);
        }
    }

    [Fact]
    public void GetNeighbors_Symmetry_AllEdgeCells_MaintainSymmetry()
    {
        var topology = new Grid2DTopology(4, 4);

        // Test edge cell at (0, 1)
        var edgeNode = new Point2D(0, 1);
        var edgeNeighbors = topology.GetNeighbors(edgeNode).ToList();

        foreach (var neighbor in edgeNeighbors)
        {
            var neighborOfNeighbor = topology.GetNeighbors(neighbor).ToList();
            Assert.Contains(edgeNode, neighborOfNeighbor);
        }
    }

    [Fact]
    public void GetNeighbors_Symmetry_AllCornerCells_MaintainSymmetry()
    {
        var topology = new Grid2DTopology(3, 3);

        var corners = new[]
        {
            new Point2D(0, 0),
            new Point2D(2, 0),
            new Point2D(0, 2),
            new Point2D(2, 2)
        };

        foreach (var corner in corners)
        {
            var cornerNeighbors = topology.GetNeighbors(corner).ToList();

            foreach (var neighbor in cornerNeighbors)
            {
                var neighborOfNeighbor = topology.GetNeighbors(neighbor).ToList();
                Assert.Contains(corner, neighborOfNeighbor);
            }
        }
    }

    #endregion

    #region ITopology Interface Tests

    [Fact]
    public void Grid2DTopology_ImplementsITopology()
    {
        var topology = new Grid2DTopology(3, 3);

        Assert.IsAssignableFrom<ITopology<Point2D>>(topology);
    }

    #endregion
}
