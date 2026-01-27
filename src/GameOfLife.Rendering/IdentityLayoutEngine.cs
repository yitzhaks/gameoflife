using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// A layout engine for RectangularTopology where identity equals coordinate.
/// Creates layouts that return the identity directly as the position.
/// </summary>
public sealed class IdentityLayoutEngine : ILayoutEngine<RectangularTopology, Point2D, Point2D, RectangularBounds>
{
    /// <summary>
    /// Creates a layout snapshot for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <returns>An immutable layout snapshot where positions equal identities.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology is null.</exception>
    public ILayout<Point2D, Point2D, RectangularBounds> CreateLayout(RectangularTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        return new IdentityLayout(topology);
    }
}
