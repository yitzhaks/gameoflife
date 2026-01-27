namespace GameOfLife.Core;

/// <summary>
/// Classic Conway's Game of Life rules (B3/S23).
/// Birth: Dead cell with exactly 3 alive neighbors becomes alive.
/// Survival: Alive cell with 2 or 3 alive neighbors stays alive.
/// Death: All other cases result in dead.
/// </summary>
public class ClassicRules : CellularAutomatonRules
{
    /// <inheritdoc />
    public override bool DefaultState => false;

    /// <inheritdoc />
    public override bool GetNextState(bool currentState, int aliveNeighborCount) =>
        currentState
            ? aliveNeighborCount is 2 or 3  // Survival
            : aliveNeighborCount is 3;      // Birth
}
