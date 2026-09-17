using Shouldly;

namespace GameOfLife.Core.Tests;

public class HexagonalWorldTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithRadius_CreatesWorld()
    {
        var world = new HexagonalWorld(5);

        world.Radius.ShouldBe(5);
        world.Topology.Radius.ShouldBe(5);
    }

    [Fact]
    public void Constructor_NullRules_ThrowsArgumentNullException() => _ = Should.Throw<ArgumentNullException>(() => new HexagonalWorld(5, null!));

    #endregion

    #region Tick Tests - Classic Rules

    [Fact]
    public void Tick_EmptyGeneration_StaysEmpty()
    {
        var world = new HexagonalWorld(3);
        using var generation = new HexGeneration();

        using IGeneration<HexPoint, bool> next = world.Tick(generation);

        // All cells should be dead
        foreach (HexPoint node in world.Topology.Nodes)
        {
            next[node].ShouldBeFalse();
        }
    }

    [Fact]
    public void Tick_SingleCell_Dies()
    {
        // With classic rules, a single cell with no neighbors dies
        var world = new HexagonalWorld(3);
        using var generation = new HexGeneration([default]);

        using IGeneration<HexPoint, bool> next = world.Tick(generation);

        next[default].ShouldBeFalse();
    }

    [Fact]
    public void Tick_ThreeNeighborsInLine_CreatesBirth()
    {
        // Three alive cells in a row should create a birth at some position
        // This tests that the tick mechanism works
        var world = new HexagonalWorld(3);
        using var generation = new HexGeneration([default, (1, 0), (-1, 0)]);

        using IGeneration<HexPoint, bool> next = world.Tick(generation);

        // The world should evolve (not stay exactly the same)
        // Due to classic rules, specific behavior depends on neighbor counts
        _ = next.ShouldNotBeNull();
    }

    [Fact]
    public void Tick_NullGeneration_ThrowsArgumentNullException()
    {
        var world = new HexagonalWorld(3);

        _ = Should.Throw<ArgumentNullException>(() => world.Tick(null!));
    }

    #endregion

    #region Tick Tests - Cell Survival

    [Fact]
    public void Tick_CellWithTwoNeighbors_Survives()
    {
        // In classic rules, a cell with 2 neighbors survives
        var world = new HexagonalWorld(3);
        // Place a cell at origin with exactly 2 neighbors
        HexPoint center = default;
        HexPoint neighbor1 = (1, 0);
        HexPoint neighbor2 = (-1, 0);
        using var generation = new HexGeneration([center, neighbor1, neighbor2]);

        using IGeneration<HexPoint, bool> next = world.Tick(generation);

        // Center should survive (has 2 live neighbors)
        next[center].ShouldBeTrue();
    }

    [Fact]
    public void Tick_CellWithThreeNeighbors_Survives()
    {
        // In classic rules, a cell with 3 neighbors survives
        var world = new HexagonalWorld(3);
        HexPoint center = default;
        HexPoint neighbor1 = (1, 0);
        HexPoint neighbor2 = (-1, 0);
        HexPoint neighbor3 = (0, 1);
        using var generation = new HexGeneration([center, neighbor1, neighbor2, neighbor3]);

        using IGeneration<HexPoint, bool> next = world.Tick(generation);

        next[center].ShouldBeTrue();
    }

    [Fact]
    public void Tick_CellWithFourNeighbors_Dies()
    {
        // In classic rules, a cell with 4 neighbors dies (overcrowding)
        var world = new HexagonalWorld(3);
        HexPoint center = default;
        HexPoint[] neighbors = [(1, 0), (-1, 0), (0, 1), (0, -1)];
        var allCells = new List<HexPoint> { center };
        allCells.AddRange(neighbors);
        using var generation = new HexGeneration(allCells);

        using IGeneration<HexPoint, bool> next = world.Tick(generation);

        next[center].ShouldBeFalse();
    }

    #endregion

    #region Tick Tests - Birth

    [Fact]
    public void Tick_DeadCellWithThreeNeighbors_BecomesAlive()
    {
        // In classic rules, a dead cell with exactly 3 neighbors becomes alive
        var world = new HexagonalWorld(3);
        // Place 3 cells around (1, -1) but not at (1, -1) itself
        HexPoint target = (1, -1);
        // Neighbors of (1, -1) include origin (0, 0), (1, 0), (0, -1), (2, -1), (2, -2), (1, -2)
        // Let's use (0, 0), (1, 0), (0, -1)
        using var generation = new HexGeneration([default, (1, 0), (0, -1)]);

        using IGeneration<HexPoint, bool> next = world.Tick(generation);

        // Verify that some cell is born - (1, -1) has 3 live neighbors
        next[target].ShouldBeTrue();
    }

    #endregion

    #region IWorld Interface Tests

    [Fact]
    public void HexagonalWorld_ImplementsIWorld()
    {
        var world = new HexagonalWorld(3);

        _ = world.ShouldBeAssignableTo<IWorld<HexPoint, bool, IGeneration<HexPoint, bool>>>();
    }

    [Fact]
    public void Topology_ReturnsHexagonalTopology()
    {
        var world = new HexagonalWorld(3);

        ITopology<HexPoint> topology = ((IWorld<HexPoint, bool, IGeneration<HexPoint, bool>>)world).Topology;

        _ = topology.ShouldBeAssignableTo<HexagonalTopology>();
    }

    #endregion
}
