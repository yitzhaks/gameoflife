using Shouldly;

namespace GameOfLife.Core.Tests;

public class HexGenerationTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_Default_CreatesEmptyGeneration()
    {
        using var generation = new HexGeneration();

        generation.AliveCells.Count.ShouldBe(0);
    }

    [Fact]
    public void Constructor_WithAliveCells_ContainsCells()
    {
        HexPoint[] aliveCells = [default, (1, 0), (0, 1)];
        using var generation = new HexGeneration(aliveCells);

        generation.AliveCells.Count.ShouldBe(3);
    }

    [Fact]
    public void Constructor_NullAliveCells_ThrowsArgumentNullException() => _ = Should.Throw<ArgumentNullException>(() => new HexGeneration(null!));

    [Fact]
    public void Constructor_DefensiveCopy_ModifyingSourceDoesNotAffectGeneration()
    {
        var aliveCells = new List<HexPoint> { default, (1, 0) };
        using var generation = new HexGeneration(aliveCells);

        aliveCells.Add((2, 0));

        generation.AliveCells.Count.ShouldBe(2);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_AliveCell_ReturnsTrue()
    {
        using var generation = new HexGeneration([default, (1, 0)]);

        generation[default].ShouldBeTrue();
        generation[(1, 0)].ShouldBeTrue();
    }

    [Fact]
    public void Indexer_DeadCell_ReturnsFalse()
    {
        using var generation = new HexGeneration([default]);

        generation[(1, 0)].ShouldBeFalse();
        generation[(5, 5)].ShouldBeFalse();
    }

    [Fact]
    public void Indexer_EmptyGeneration_AllCellsDead()
    {
        using var generation = new HexGeneration();

        generation[default].ShouldBeFalse();
        generation[(1, 0)].ShouldBeFalse();
        generation[(10, 10)].ShouldBeFalse();
    }

    #endregion

    #region AliveCells Tests

    [Fact]
    public void AliveCells_ReturnsReadOnlySet()
    {
        using var generation = new HexGeneration([(1, 0), (0, 1)]);

        _ = generation.AliveCells.ShouldBeAssignableTo<IReadOnlySet<HexPoint>>();
    }

    [Fact]
    public void AliveCells_ContainsExpectedCells()
    {
        HexPoint[] cells = [default, (1, 0), (-1, 1)];
        using var generation = new HexGeneration(cells);

        foreach (HexPoint cell in cells)
        {
            generation.AliveCells.Contains(cell).ShouldBeTrue();
        }
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var generation = new HexGeneration([default]);

        // Should not throw
        generation.Dispose();
        generation.Dispose();
    }

    #endregion

    #region IGeneration Interface Tests

    [Fact]
    public void HexGeneration_ImplementsIGeneration()
    {
        using var generation = new HexGeneration();

        _ = generation.ShouldBeAssignableTo<IGeneration<HexPoint, bool>>();
    }

    #endregion
}
