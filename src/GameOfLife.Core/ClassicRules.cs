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
    public bool GetNextState(bool currentState, IEnumerable<bool> neighborStates)
    {
        ArgumentNullException.ThrowIfNull(neighborStates);

        int aliveNeighbors = neighborStates.Count(alive => alive);

        if (currentState)
        {
            // Survival: alive cell with 2 or 3 neighbors survives
            return aliveNeighbors is 2 or 3;
        }
        else
        {
            // Birth: dead cell with exactly 3 neighbors becomes alive
            return aliveNeighbors is 3;
        }
    }
}
