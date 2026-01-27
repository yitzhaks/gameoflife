namespace GameOfLife.Core;

/// <summary>
/// Represents the state of all nodes at a single point in time.
/// A generation is immutable - advancing time creates a new generation.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes. Must be equatable for O(1) lookups.</typeparam>
/// <typeparam name="TState">The type representing the state of each node.</typeparam>
public interface IGeneration<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    /// <summary>
    /// Gets the state of the specified node.
    /// </summary>
    /// <param name="node">The node to get state for.</param>
    /// <returns>The state of the node.</returns>
    TState this[TIdentity node] { get; }
}
