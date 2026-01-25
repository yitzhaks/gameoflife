namespace GameOfLife.Core.Tests;

public class GridTopologyTests
{
    [Fact]
    public void Constructor_InvalidWidth_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new GridTopology(0, 1));

        Assert.Equal("width", exception.ParamName);
    }

    [Fact]
    public void Constructor_InvalidHeight_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new GridTopology(1, 0));

        Assert.Equal("height", exception.ParamName);
    }

    [Fact]
    public void Nodes_ReturnsAllPointsInGrid()
    {
        var topology = new GridTopology(2, 2);

        var nodes = topology.Nodes.ToHashSet();

        Assert.Equal(4, nodes.Count);
        Assert.Contains(new Point2D(0, 0), nodes);
        Assert.Contains(new Point2D(1, 0), nodes);
        Assert.Contains(new Point2D(0, 1), nodes);
        Assert.Contains(new Point2D(1, 1), nodes);
    }

    [Fact]
    public void GetNeighbors_CenterCell_ReturnsEightNeighbors()
    {
        var topology = new GridTopology(3, 3);

        var neighbors = topology.GetNeighbors(new Point2D(1, 1)).ToHashSet();

        Assert.Equal(8, neighbors.Count);
        Assert.DoesNotContain(new Point2D(1, 1), neighbors);
    }

    [Fact]
    public void GetNeighbors_CornerCell_ClipsToBounds()
    {
        var topology = new GridTopology(3, 3);

        var neighbors = topology.GetNeighbors(new Point2D(0, 0)).ToHashSet();

        Assert.Equal(3, neighbors.Count);
        Assert.Contains(new Point2D(0, 1), neighbors);
        Assert.Contains(new Point2D(1, 0), neighbors);
        Assert.Contains(new Point2D(1, 1), neighbors);
    }

    [Fact]
    public void GetNeighbors_UnknownNode_Throws()
    {
        var topology = new GridTopology(2, 2);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => topology.GetNeighbors(new Point2D(3, 3)).ToList());

        Assert.Equal("node", exception.ParamName);
    }
}
