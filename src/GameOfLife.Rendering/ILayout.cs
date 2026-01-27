namespace GameOfLife.Rendering;

/// <summary>
/// Represents an immutable layout snapshot bound to a topology.
/// Maps node identities to layout coordinates and provides bounds information.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TCoordinate">The coordinate type used to represent positions.</typeparam>
/// <typeparam name="TBounds">The bounds type for the layout region.</typeparam>
public interface ILayout<TIdentity, TCoordinate, TBounds>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
    where TBounds : IBounds<TCoordinate>
{
    /// <summary>
    /// Enumerates nodes in the specified order.
    /// </summary>
    /// <param name="order">The comparer defining the enumeration order.</param>
    /// <returns>The nodes in the specified order.</returns>
    IEnumerable<TIdentity> EnumerateNodes(IComparer<TIdentity> order);

    /// <summary>
    /// Gets the positions of nodes in the layout.
    /// </summary>
    ILayoutPositions<TIdentity, TCoordinate> Positions { get; }

    /// <summary>
    /// Gets the bounds of the layout region.
    /// </summary>
    TBounds Bounds { get; }
}
