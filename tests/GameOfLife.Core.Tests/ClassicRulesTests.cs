using Shouldly;

namespace GameOfLife.Core.Tests;

public class ClassicRulesTests
{
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

    [Theory]
    [AutoNSubstituteData]
    public void DefaultState_ReturnsDeadState(ClassicRules rules)
    {
        // Arrange & Act
        bool defaultState = rules.DefaultState;

        // Assert
        defaultState.ShouldBeFalse();
    }

    #endregion

    #region Dead Cell Tests (Birth Rule - B3)

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_DeadCellWithZeroNeighbors_StaysDead(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = false;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 0);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_DeadCellWithOneNeighbor_StaysDead(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = false;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 1);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_DeadCellWithTwoNeighbors_StaysDead(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = false;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 2);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_DeadCellWithThreeNeighbors_BecomesAlive(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = false;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 3);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_DeadCellWithFourNeighbors_StaysDead(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = false;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 4);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_DeadCellWithFiveNeighbors_StaysDead(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = false;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 5);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_DeadCellWithEightNeighbors_StaysDead(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = false;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 8);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Alive Cell Tests (Survival Rule - S23)

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithZeroNeighbors_Dies(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 0);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithOneNeighbor_Dies(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 1);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithTwoNeighbors_Survives(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 2);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithThreeNeighbors_Survives(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 3);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithFourNeighbors_Dies(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 4);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithFiveNeighbors_Dies(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 5);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithEightNeighbors_Dies(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 8);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Mixed Neighbor State Tests

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_DeadCellWithThreeAliveAndTwoDeadNeighbors_BecomesAlive(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = false;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 3, deadCount: 2);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithTwoAliveAndFiveDeadNeighbors_Survives(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 2, deadCount: 5);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetNextState_AliveCellWithThreeAliveAndFourDeadNeighbors_Survives(ClassicRules rules)
    {
        // Arrange
        IRules<bool> rulesInterface = rules;
        bool current = true;
        List<bool> neighbors = CreateNeighborStates(aliveCount: 3, deadCount: 4);

        // Act
        bool result = rulesInterface.GetNextState(current, neighbors);

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region IRules Interface Tests

    [Theory]
    [AutoNSubstituteData]
    public void ClassicRules_ImplementsIRulesInterface(ClassicRules rules) =>
        _ = rules.ShouldBeAssignableTo<IRules<bool>>();

    #endregion
}
