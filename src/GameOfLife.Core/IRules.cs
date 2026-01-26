namespace GameOfLife.Core;

/// <summary>
/// Defines the rules for state transitions in a cellular automaton.
/// </summary>
/// <typeparam name="TState">The type representing cell state.</typeparam>
public interface IRules<TState>
{
    /// <summary>
    /// Gets the default state for cells that have no explicit state.
    /// </summary>
    TState DefaultState { get; }

    /// <summary>
    /// Computes the next state for a cell based on its current state and the states of its neighbors.
    /// </summary>
    /// <param name="current">The current state of the cell.</param>
    /// <param name="neighborStates">The states of all neighboring cells.</param>
    /// <returns>The next state for the cell.</returns>
    TState GetNextState(TState current, IEnumerable<TState> neighborStates);
}
