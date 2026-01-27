namespace GameOfLife.Core;

/// <summary>
/// Defines the structure of a cellular automaton space.
/// A topology describes what nodes exist and how they connect (neighbors),
/// but contains no state information.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes. Must be equatable for O(1) lookups.</typeparam>
public interface ITopology<TIdentity> where TIdentity : notnull, IEquatable<TIdentity>
{
    /// <summary>
    /// Gets all nodes in the topology.
    /// </summary>
    IEnumerable<TIdentity> Nodes { get; }

    /// <summary>
    /// Gets the neighbors of a specified node.
    /// </summary>
    /// <param name="node">The node to get neighbors for.</param>
    /// <returns>The neighboring nodes.</returns>
    IEnumerable<TIdentity> GetNeighbors(TIdentity node);
}
