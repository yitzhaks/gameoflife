using Shouldly;

namespace GameOfLife.Core.Tests;

public class HexagonalTopologyTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_RadiusZero_CreatesSingleCell()
    {
        var topology = new HexagonalTopology(0);

        topology.Radius.ShouldBe(0);
        topology.CellCount.ShouldBe(1);
    }

    [Fact]
    public void Constructor_RadiusOne_Creates7Cells()
    {
        var topology = new HexagonalTopology(1);

        topology.Radius.ShouldBe(1);
        topology.CellCount.ShouldBe(7); // 3*1*2 + 1 = 7
    }

    [Fact]
    public void Constructor_RadiusTwo_Creates19Cells()
    {
        var topology = new HexagonalTopology(2);

        topology.Radius.ShouldBe(2);
        topology.CellCount.ShouldBe(19); // 3*2*3 + 1 = 19
    }

    [Fact]
    public void Constructor_RadiusThree_Creates37Cells()
    {
        var topology = new HexagonalTopology(3);

        topology.Radius.ShouldBe(3);
        topology.CellCount.ShouldBe(37); // 3*3*4 + 1 = 37
    }

    [Fact]
    public void Constructor_NegativeRadius_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            new HexagonalTopology(-1));

        exception.ParamName.ShouldBe("radius");
    }

    #endregion

    #region Nodes Property Tests

    [Fact]
    public void Nodes_RadiusZero_ContainsOnlyOrigin()
    {
        var topology = new HexagonalTopology(0);
        var nodes = topology.Nodes.ToList();

        _ = nodes.ShouldHaveSingleItem();
        nodes[0].ShouldBe(default);
    }

    [Fact]
    public void Nodes_RadiusOne_Contains7Points()
    {
        var topology = new HexagonalTopology(1);
        var nodes = topology.Nodes.ToList();

        nodes.Count.ShouldBe(7);
    }

    [Fact]
    public void Nodes_RadiusOne_ContainsOriginAndAllNeighbors()
    {
        var topology = new HexagonalTopology(1);
        var nodes = topology.Nodes.ToList();

        HexPoint[] expectedPoints =
        [
            default,
            (1, 0),
            (-1, 0),
            (1, -1),
            (0, -1),
            (0, 1),
            (-1, 1)
        ];

        foreach (HexPoint expected in expectedPoints)
        {
            nodes.ShouldContain(expected);
        }
    }

    [Fact]
    public void Nodes_Count_MatchesCellCount()
    {
        var topology = new HexagonalTopology(5);

        topology.Nodes.Count().ShouldBe(topology.CellCount);
    }

    [Fact]
    public void Nodes_AllPointsWithinRadius()
    {
        var topology = new HexagonalTopology(3);

        foreach (HexPoint node in topology.Nodes)
        {
            node.IsWithinRadius(3).ShouldBeTrue();
        }
    }

    #endregion

    #region GetNeighbors Tests - Valid Nodes

    [Fact]
    public void GetNeighbors_Origin_Has6NeighborsWithRadius1()
    {
        var topology = new HexagonalTopology(2);
        var neighbors = topology.GetNeighbors(default).ToList();

        neighbors.Count.ShouldBe(6);
    }

    [Fact]
    public void GetNeighbors_Origin_ContainsAll6Directions()
    {
        var topology = new HexagonalTopology(2);
        var neighbors = topology.GetNeighbors(default).ToList();

        HexPoint[] expectedNeighbors =
        [
            (1, 0),
            (-1, 0),
            (1, -1),
            (0, -1),
            (0, 1),
            (-1, 1)
        ];

        foreach (HexPoint expected in expectedNeighbors)
        {
            neighbors.ShouldContain(expected);
        }
    }

    [Fact]
    public void GetNeighbors_EdgeCell_HasFewerNeighbors()
    {
        var topology = new HexagonalTopology(1);
        // (1, 0) is on the edge
        var neighbors = topology.GetNeighbors((1, 0)).ToList();

        neighbors.Count.ShouldBeLessThan(6);
    }

    [Fact]
    public void GetNeighbors_RadiusZero_OriginHasNoNeighbors()
    {
        var topology = new HexagonalTopology(0);
        var neighbors = topology.GetNeighbors(default).ToList();

        neighbors.ShouldBeEmpty();
    }

    #endregion

    #region GetNeighbors Tests - Invalid Nodes

    [Fact]
    public void GetNeighbors_NodeOutsideTopology_ThrowsArgumentOutOfRangeException()
    {
        var topology = new HexagonalTopology(1);

        ArgumentOutOfRangeException exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            topology.GetNeighbors((5, 5)).ToList());

        exception.ParamName.ShouldBe("node");
    }

    #endregion

    #region Symmetry Tests

    [Fact]
    public void GetNeighbors_Symmetry_IfAIsNeighborOfB_ThenBIsNeighborOfA()
    {
        var topology = new HexagonalTopology(3);

        HexPoint centerNode = default;
        var centerNeighbors = topology.GetNeighbors(centerNode).ToList();

        foreach (HexPoint neighbor in centerNeighbors)
        {
            topology.GetNeighbors(neighbor).ShouldContain(centerNode);
        }
    }

    [Fact]
    public void GetNeighbors_AllNodes_MaintainSymmetry()
    {
        var topology = new HexagonalTopology(2);

        foreach (HexPoint node in topology.Nodes)
        {
            var nodeNeighbors = topology.GetNeighbors(node).ToList();

            foreach (HexPoint neighbor in nodeNeighbors)
            {
                topology.GetNeighbors(neighbor).ShouldContain(node);
            }
        }
    }

    #endregion

    #region Contains Tests

    [Fact]
    public void Contains_OriginWithAnyRadius_ReturnsTrue()
    {
        var topology = new HexagonalTopology(5);

        topology.Contains(default).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointWithinRadius_ReturnsTrue()
    {
        var topology = new HexagonalTopology(3);

        topology.Contains((2, 1)).ShouldBeTrue();
    }

    [Fact]
    public void Contains_PointOutsideRadius_ReturnsFalse()
    {
        var topology = new HexagonalTopology(3);

        topology.Contains((10, 10)).ShouldBeFalse();
    }

    #endregion

    #region Stack-Allocated Enumerator Tests

    [Fact]
    public void GetNeighborsStack_ReturnsSameNeighborsAsGetNeighbors()
    {
        var topology = new HexagonalTopology(3);
        HexPoint center = default;

        var heapNeighbors = topology.GetNeighbors(center).ToList();
        var stackNeighbors = new List<HexPoint>();

        foreach (HexPoint neighbor in topology.GetNeighborsStack(center))
        {
            stackNeighbors.Add(neighbor);
        }

        stackNeighbors.Count.ShouldBe(heapNeighbors.Count);
        foreach (HexPoint neighbor in heapNeighbors)
        {
            stackNeighbors.ShouldContain(neighbor);
        }
    }

    [Fact]
    public void GetNeighborsStack_EdgeCell_ReturnsCorrectCount()
    {
        var topology = new HexagonalTopology(2);
        HexPoint edgeCell = (2, 0);

        int count = 0;
        foreach (HexPoint _ in topology.GetNeighborsStack(edgeCell))
        {
            count++;
        }

        count.ShouldBe(topology.GetNeighbors(edgeCell).Count());
    }

    #endregion

    #region ITopology Interface Tests

    [Fact]
    public void HexagonalTopology_ImplementsITopology()
    {
        var topology = new HexagonalTopology(3);

        _ = topology.ShouldBeAssignableTo<ITopology<HexPoint>>();
    }

    #endregion
}
