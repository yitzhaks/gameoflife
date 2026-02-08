using Shouldly;

namespace GameOfLife.Core.Tests;

public class HexRulesTests
{
    #region HexRulesB2S34 Tests (Birth: 2, Survival: 3-4)

    [Fact]
    public void HexRulesB2S34_DefaultState_ReturnsFalse()
    {
        var rules = new HexRulesB2S34();

        rules.DefaultState.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0, false)] // No birth with 0 neighbors
    [InlineData(1, false)] // No birth with 1 neighbor
    [InlineData(2, true)]  // Birth with 2 neighbors
    [InlineData(3, false)] // No birth with 3 neighbors
    [InlineData(4, false)] // No birth with 4 neighbors
    [InlineData(5, false)] // No birth with 5 neighbors
    [InlineData(6, false)] // No birth with 6 neighbors
    public void HexRulesB2S34_DeadCell_BirthOnlyWithTwoNeighbors(int aliveNeighbors, bool expected)
    {
        var rules = new HexRulesB2S34();

        bool result = rules.GetNextState(false, aliveNeighbors);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0, false)] // Die with 0 neighbors
    [InlineData(1, false)] // Die with 1 neighbor
    [InlineData(2, false)] // Die with 2 neighbors
    [InlineData(3, true)]  // Survive with 3 neighbors
    [InlineData(4, true)]  // Survive with 4 neighbors
    [InlineData(5, false)] // Die with 5 neighbors
    [InlineData(6, false)] // Die with 6 neighbors
    public void HexRulesB2S34_AliveCell_SurviveWithThreeOrFourNeighbors(int aliveNeighbors, bool expected)
    {
        var rules = new HexRulesB2S34();

        bool result = rules.GetNextState(true, aliveNeighbors);

        result.ShouldBe(expected);
    }

    [Fact]
    public void HexRulesB2S34_ImplementsICellularAutomatonRules()
    {
        var rules = new HexRulesB2S34();

        _ = rules.ShouldBeAssignableTo<ICellularAutomatonRules>();
    }

    #endregion

    #region HexRulesB2S35 Tests (Birth: 2, Survival: 3-5)

    [Fact]
    public void HexRulesB2S35_DefaultState_ReturnsFalse()
    {
        var rules = new HexRulesB2S35();

        rules.DefaultState.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0, false)] // No birth with 0 neighbors
    [InlineData(1, false)] // No birth with 1 neighbor
    [InlineData(2, true)]  // Birth with 2 neighbors
    [InlineData(3, false)] // No birth with 3 neighbors
    [InlineData(4, false)] // No birth with 4 neighbors
    [InlineData(5, false)] // No birth with 5 neighbors
    [InlineData(6, false)] // No birth with 6 neighbors
    public void HexRulesB2S35_DeadCell_BirthOnlyWithTwoNeighbors(int aliveNeighbors, bool expected)
    {
        var rules = new HexRulesB2S35();

        bool result = rules.GetNextState(false, aliveNeighbors);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0, false)] // Die with 0 neighbors
    [InlineData(1, false)] // Die with 1 neighbor
    [InlineData(2, false)] // Die with 2 neighbors
    [InlineData(3, true)]  // Survive with 3 neighbors
    [InlineData(4, true)]  // Survive with 4 neighbors
    [InlineData(5, true)]  // Survive with 5 neighbors
    [InlineData(6, false)] // Die with 6 neighbors
    public void HexRulesB2S35_AliveCell_SurviveWithThreeToFiveNeighbors(int aliveNeighbors, bool expected)
    {
        var rules = new HexRulesB2S35();

        bool result = rules.GetNextState(true, aliveNeighbors);

        result.ShouldBe(expected);
    }

    [Fact]
    public void HexRulesB2S35_ImplementsICellularAutomatonRules()
    {
        var rules = new HexRulesB2S35();

        _ = rules.ShouldBeAssignableTo<ICellularAutomatonRules>();
    }

    #endregion

    #region HexRulesB24S35 Tests (Birth: 2 or 4, Survival: 3-5)

    [Fact]
    public void HexRulesB24S35_DefaultState_ReturnsFalse()
    {
        var rules = new HexRulesB24S35();

        rules.DefaultState.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0, false)] // No birth with 0 neighbors
    [InlineData(1, false)] // No birth with 1 neighbor
    [InlineData(2, true)]  // Birth with 2 neighbors
    [InlineData(3, false)] // No birth with 3 neighbors
    [InlineData(4, true)]  // Birth with 4 neighbors
    [InlineData(5, false)] // No birth with 5 neighbors
    [InlineData(6, false)] // No birth with 6 neighbors
    public void HexRulesB24S35_DeadCell_BirthWithTwoOrFourNeighbors(int aliveNeighbors, bool expected)
    {
        var rules = new HexRulesB24S35();

        bool result = rules.GetNextState(false, aliveNeighbors);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0, false)] // Die with 0 neighbors
    [InlineData(1, false)] // Die with 1 neighbor
    [InlineData(2, false)] // Die with 2 neighbors
    [InlineData(3, true)]  // Survive with 3 neighbors
    [InlineData(4, true)]  // Survive with 4 neighbors
    [InlineData(5, true)]  // Survive with 5 neighbors
    [InlineData(6, false)] // Die with 6 neighbors
    public void HexRulesB24S35_AliveCell_SurviveWithThreeToFiveNeighbors(int aliveNeighbors, bool expected)
    {
        var rules = new HexRulesB24S35();

        bool result = rules.GetNextState(true, aliveNeighbors);

        result.ShouldBe(expected);
    }

    [Fact]
    public void HexRulesB24S35_ImplementsICellularAutomatonRules()
    {
        var rules = new HexRulesB24S35();

        _ = rules.ShouldBeAssignableTo<ICellularAutomatonRules>();
    }

    #endregion

    #region HexRulesB2S23 Tests (Birth: 2, Survival: 2-3)

    [Fact]
    public void HexRulesB2S23_DefaultState_ReturnsFalse()
    {
        var rules = new HexRulesB2S23();

        rules.DefaultState.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0, false)] // No birth with 0 neighbors
    [InlineData(1, false)] // No birth with 1 neighbor
    [InlineData(2, true)]  // Birth with 2 neighbors
    [InlineData(3, false)] // No birth with 3 neighbors
    [InlineData(4, false)] // No birth with 4 neighbors
    [InlineData(5, false)] // No birth with 5 neighbors
    [InlineData(6, false)] // No birth with 6 neighbors
    public void HexRulesB2S23_DeadCell_BirthOnlyWithTwoNeighbors(int aliveNeighbors, bool expected)
    {
        var rules = new HexRulesB2S23();

        bool result = rules.GetNextState(false, aliveNeighbors);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0, false)] // Die with 0 neighbors
    [InlineData(1, false)] // Die with 1 neighbor
    [InlineData(2, true)]  // Survive with 2 neighbors
    [InlineData(3, true)]  // Survive with 3 neighbors
    [InlineData(4, false)] // Die with 4 neighbors
    [InlineData(5, false)] // Die with 5 neighbors
    [InlineData(6, false)] // Die with 6 neighbors
    public void HexRulesB2S23_AliveCell_SurviveWithTwoOrThreeNeighbors(int aliveNeighbors, bool expected)
    {
        var rules = new HexRulesB2S23();

        bool result = rules.GetNextState(true, aliveNeighbors);

        result.ShouldBe(expected);
    }

    [Fact]
    public void HexRulesB2S23_ImplementsICellularAutomatonRules()
    {
        var rules = new HexRulesB2S23();

        _ = rules.ShouldBeAssignableTo<ICellularAutomatonRules>();
    }

    #endregion
}
