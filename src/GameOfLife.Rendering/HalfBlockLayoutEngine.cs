using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// A layout engine for RectangularTopology that creates half-block layouts.
/// Half-block layouts pack two vertically-adjacent cells into a single output position,
/// enabling the use of Unicode half-block characters for double vertical resolution.
/// </summary>
public sealed class HalfBlockLayoutEngine : ILayoutEngine<RectangularTopology, Point2D, PackedPoint2D, PackedBounds>
{
    /// <summary>
    /// Creates a layout snapshot for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <returns>An immutable layout snapshot with packed coordinates.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the topology height is odd.</exception>
    public ILayout<Point2D, PackedPoint2D, PackedBounds> CreateLayout(RectangularTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        return new HalfBlockLayout(topology);
    }
}
