namespace GameOfLife.Core;

/// <summary>
/// Rules for binary (alive/dead) cellular automaton that support optimized count-based computation.
/// </summary>
public interface ICellularAutomatonRules : IRules<bool>
{
    /// <summary>
    /// Computes the next state based on the current state and pre-counted alive neighbors.
    /// This method avoids allocation when neighbor count is already known.
    /// </summary>
    /// <param name="currentState">The current state of the cell.</param>
    /// <param name="aliveNeighborCount">The number of alive neighbors.</param>
    /// <returns>The next state for the cell.</returns>
    bool GetNextState(bool currentState, int aliveNeighborCount);
}
