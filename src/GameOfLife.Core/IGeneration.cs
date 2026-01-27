namespace GameOfLife.Core;

/// <summary>
/// Represents the state of all nodes at a single point in time.
/// A generation is immutable - advancing time creates a new generation.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes. Must be equatable for O(1) lookups.</typeparam>
/// <typeparam name="TState">The type representing the state of each node.</typeparam>
public interface IGeneration<TIdentity, TState> : IDisposable where TIdentity : notnull, IEquatable<TIdentity>
{
    /// <summary>
    /// Gets the state of the specified node.
    /// </summary>
    /// <param name="node">The node to get state for.</param>
    /// <returns>The state of the node.</returns>
    TState this[TIdentity node] { get; }
}

/// <summary>
/// Extension methods for <see cref="IGeneration{TIdentity, TState}"/>.
/// </summary>
public static class GenerationExtensions
{
    /// <summary>
    /// Gets the states of all neighbors of the specified node.
    /// </summary>
    /// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
    /// <typeparam name="TState">The type representing the state of each node.</typeparam>
    /// <param name="generation">The generation to get states from.</param>
    /// <param name="topology">The topology defining neighbors.</param>
    /// <param name="node">The node whose neighbors to query.</param>
    /// <returns>The states of all neighboring nodes.</returns>
    public static IEnumerable<TState> GetNeighborStates<TIdentity, TState>(
        this IGeneration<TIdentity, TState> generation,
        ITopology<TIdentity> topology,
        TIdentity node)
        where TIdentity : notnull, IEquatable<TIdentity>
    {
        ArgumentNullException.ThrowIfNull(topology);
        return topology.GetNeighbors(node).Select(n => generation[n]);
    }
}
