namespace GameOfLife.Rendering;

/// <summary>
/// Provides access to the layout positions of nodes in a topology.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TCoordinate">The coordinate type used to represent positions.</typeparam>
public interface ILayoutPositions<TIdentity, TCoordinate>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
{
    /// <summary>
    /// Gets the layout position of the specified node.
    /// </summary>
    /// <param name="node">The node to get the position for.</param>
    /// <returns>The layout position of the node.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the node is unknown to the layout.</exception>
    TCoordinate this[TIdentity node] { get; }
}
