namespace GameOfLife.Core;

/// <summary>
/// A dictionary-backed generation that stores state sparsely.
/// Returns a default state for nodes not explicitly stored.
/// This approach works well for Game of Life where dead cells don't need explicit storage.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TState">The type representing cell state.</typeparam>
public class DictionaryGeneration<TIdentity, TState> : IGeneration<TIdentity, TState>
    where TIdentity : notnull, IEquatable<TIdentity>
{
    private readonly Dictionary<TIdentity, TState> _states;
    private readonly TState _defaultState;

    /// <summary>
    /// Creates a new dictionary-backed generation.
    /// </summary>
    /// <param name="states">The states for nodes. The dictionary is copied defensively to ensure immutability.</param>
    /// <param name="defaultState">The default state for nodes not in the dictionary.</param>
    public DictionaryGeneration(IReadOnlyDictionary<TIdentity, TState> states, TState defaultState)
    {
        ArgumentNullException.ThrowIfNull(states);

        // Defensive copy to ensure immutability - external changes to the original dictionary
        // will not affect this generation
        _states = new Dictionary<TIdentity, TState>(states);
        _defaultState = defaultState;
    }

    /// <summary>
    /// Gets the state of a node. Returns the default state if the node is not explicitly stored.
    /// </summary>
    public TState this[TIdentity node] => _states.TryGetValue(node, out TState? state) ? state : _defaultState;
}
