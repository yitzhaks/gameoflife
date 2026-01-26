namespace GameOfLife.Core.Tests;

public class ClassicRulesTests
{
    private readonly ClassicRules _rules = new();

    #region Helper Methods

    /// <summary>
    /// Creates a list of neighbor states with the specified number of alive and dead neighbors.
    /// </summary>
    private static List<bool> CreateNeighborStates(int aliveCount, int deadCount = 0)
    {
        var neighbors = new List<bool>();
        for (int i = 0; i < aliveCount; i++)
        {
            neighbors.Add(true);
        }

        for (int i = 0; i < deadCount; i++)
        {
            neighbors.Add(false);
        }

        return neighbors;
    }

    #endregion

    #region DefaultState Tests

    [Fact]
    public void DefaultState_ReturnsDeadState()
    {
        // Arrange & Act
        var defaultState = _rules.DefaultState;

        // Assert
        Assert.False(defaultState);
    }

    #endregion

    #region Dead Cell Tests (Birth Rule - B3)

    [Fact]
    public void GetNextState_DeadCellWithZeroNeighbors_StaysDead()
    {
        // Arrange
        var current = false;
        var neighbors = CreateNeighborStates(aliveCount: 0);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_DeadCellWithOneNeighbor_StaysDead()
    {
        // Arrange
        var current = false;
        var neighbors = CreateNeighborStates(aliveCount: 1);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_DeadCellWithTwoNeighbors_StaysDead()
    {
        // Arrange
        var current = false;
        var neighbors = CreateNeighborStates(aliveCount: 2);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_DeadCellWithThreeNeighbors_BecomesAlive()
    {
        // Arrange
        var current = false;
        var neighbors = CreateNeighborStates(aliveCount: 3);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetNextState_DeadCellWithFourNeighbors_StaysDead()
    {
        // Arrange
        var current = false;
        var neighbors = CreateNeighborStates(aliveCount: 4);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_DeadCellWithFiveNeighbors_StaysDead()
    {
        // Arrange
        var current = false;
        var neighbors = CreateNeighborStates(aliveCount: 5);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_DeadCellWithEightNeighbors_StaysDead()
    {
        // Arrange
        var current = false;
        var neighbors = CreateNeighborStates(aliveCount: 8);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Alive Cell Tests (Survival Rule - S23)

    [Fact]
    public void GetNextState_AliveCellWithZeroNeighbors_Dies()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 0);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_AliveCellWithOneNeighbor_Dies()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 1);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_AliveCellWithTwoNeighbors_Survives()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 2);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetNextState_AliveCellWithThreeNeighbors_Survives()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 3);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetNextState_AliveCellWithFourNeighbors_Dies()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 4);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_AliveCellWithFiveNeighbors_Dies()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 5);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNextState_AliveCellWithEightNeighbors_Dies()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 8);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Mixed Neighbor State Tests

    [Fact]
    public void GetNextState_DeadCellWithThreeAliveAndTwoDeadNeighbors_BecomesAlive()
    {
        // Arrange
        var current = false;
        var neighbors = CreateNeighborStates(aliveCount: 3, deadCount: 2);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetNextState_AliveCellWithTwoAliveAndFiveDeadNeighbors_Survives()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 2, deadCount: 5);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetNextState_AliveCellWithThreeAliveAndFourDeadNeighbors_Survives()
    {
        // Arrange
        var current = true;
        var neighbors = CreateNeighborStates(aliveCount: 3, deadCount: 4);

        // Act
        var result = _rules.GetNextState(current, neighbors);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region IRules Interface Tests

    [Fact]
    public void ClassicRules_ImplementsIRulesInterface()
    {
        // Arrange & Act
        var rules = new ClassicRules();

        // Assert
        Assert.IsAssignableFrom<IRules<bool>>(rules);
    }

    #endregion
}
