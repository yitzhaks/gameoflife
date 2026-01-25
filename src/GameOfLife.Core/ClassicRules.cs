namespace GameOfLife.Core;

/// <summary>
/// Classic Conway's Game of Life rules (B3/S23).
/// Birth: Dead cell with exactly 3 alive neighbors becomes alive.
/// Survival: Alive cell with 2 or 3 alive neighbors stays alive.
/// Death: All other cases result in dead.
/// </summary>
public class ClassicRules : IRules<bool>
{
    /// <summary>
    /// Gets the default state (dead/false).
    /// </summary>
    public bool DefaultState => false;

    /// <summary>
    /// Computes the next state based on B3/S23 rules.
    /// </summary>
    public bool GetNextState(bool current, IEnumerable<bool> neighborStates)
    {
        var aliveNeighbors = neighborStates.Count(s => s);

        // Survival (alive with 2-3 neighbors) or Birth (dead with exactly 3 neighbors)
        return current ? aliveNeighbors is 2 or 3 : aliveNeighbors == 3;
    }
}
