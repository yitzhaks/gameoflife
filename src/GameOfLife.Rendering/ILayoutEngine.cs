using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// Creates layout snapshots from topologies.
/// Layout engines are stateless and reusable.
/// </summary>
/// <typeparam name="TTopology">The topology type that this engine accepts.</typeparam>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TCoordinate">The coordinate type used to represent positions.</typeparam>
/// <typeparam name="TBounds">The bounds type for the layout region.</typeparam>
public interface ILayoutEngine<TTopology, TIdentity, TCoordinate, TBounds>
    where TTopology : ITopology<TIdentity>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
    where TBounds : IBounds<TCoordinate>
{
    /// <summary>
    /// Creates a layout snapshot for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <returns>An immutable layout snapshot.</returns>
    ILayout<TIdentity, TCoordinate, TBounds> CreateLayout(TTopology topology);
}
