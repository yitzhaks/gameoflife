namespace GameOfLife.Core;

public sealed class ConwayRules : IRules<bool>
{
    public bool DefaultState => false;

    public bool GetNextState(bool current, IEnumerable<bool> neighborStates)
    {
        var aliveNeighbors = 0;
        foreach (var neighborState in neighborStates)
        {
            if (neighborState)
            {
                aliveNeighbors++;
            }
        }

        return current ? aliveNeighbors is 2 or 3 : aliveNeighbors == 3;
    }
}
