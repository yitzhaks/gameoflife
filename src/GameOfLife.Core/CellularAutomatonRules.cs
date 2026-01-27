namespace GameOfLife.Core;

/// <summary>
/// Base class for binary (alive/dead) cellular automaton rules.
/// Provides the <see cref="IRules{TState}"/> implementation that delegates to the count-based method.
/// </summary>
public abstract class CellularAutomatonRules : ICellularAutomatonRules
{
    /// <inheritdoc />
    public abstract bool DefaultState { get; }

    /// <inheritdoc />
    bool IRules<bool>.GetNextState(bool currentState, IEnumerable<bool> neighborStates)
    {
        ArgumentNullException.ThrowIfNull(neighborStates);

        int aliveNeighborCount = neighborStates.Count(alive => alive);
        return GetNextState(currentState, aliveNeighborCount);
    }

    /// <inheritdoc />
    public abstract bool GetNextState(bool currentState, int aliveNeighborCount);
}
