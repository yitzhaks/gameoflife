using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// A layout engine for hexagonal topologies that maps HexPoint identities to Point2D output coordinates.
/// </summary>
public sealed class HexLayoutEngine : ILayoutEngine<HexagonalTopology, HexPoint, Point2D, HexBounds>
{
    /// <summary>
    /// Creates a new hex layout engine.
    /// </summary>
    public HexLayoutEngine()
    {
    }

    /// <summary>
    /// Creates a layout snapshot for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <returns>An immutable layout snapshot.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology is null.</exception>
    public ILayout<HexPoint, Point2D, HexBounds> CreateLayout(HexagonalTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        return new HexLayout(topology);
    }
}
