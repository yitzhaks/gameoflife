namespace GameOfLife.Core.Tests;

public class ConwayRulesTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public void GetNextState_DeadCell_EvolvesAccordingToBirthRule(int aliveNeighbors, bool expected)
    {
        var rules = new ConwayRules();
        var neighbors = CreateNeighbors(aliveNeighbors);

        var next = rules.GetNextState(false, neighbors);

        Assert.Equal(expected, next);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public void GetNextState_AliveCell_EvolvesAccordingToSurvivalRule(int aliveNeighbors, bool expected)
    {
        var rules = new ConwayRules();
        var neighbors = CreateNeighbors(aliveNeighbors);

        var next = rules.GetNextState(true, neighbors);

        Assert.Equal(expected, next);
    }

    private static IEnumerable<bool> CreateNeighbors(int aliveNeighbors)
    {
        for (var i = 0; i < aliveNeighbors; i++)
        {
            yield return true;
        }

        for (var i = aliveNeighbors; i < 8; i++)
        {
            yield return false;
        }
    }
}
