using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// Provides position lookup for half-block layouts.
/// Maps Point2D identities to PackedPoint2D coordinates where two vertically-adjacent cells
/// are packed into a single output position.
/// </summary>
public sealed class HalfBlockLayoutPositions : ILayoutPositions<Point2D, PackedPoint2D>
{
    private readonly PackedBounds _bounds;

    /// <summary>
    /// Creates a new half-block layout positions instance.
    /// </summary>
    /// <param name="bounds">The bounds defining valid positions.</param>
    public HalfBlockLayoutPositions(PackedBounds bounds)
    {
        ArgumentNullException.ThrowIfNull(bounds);

        _bounds = bounds;
    }

    /// <summary>
    /// Gets the layout position of the specified node.
    /// Maps the node's Y coordinate to packed space (Y / 2) and determines
    /// whether it's the top or bottom cell of the pair.
    /// </summary>
    /// <param name="node">The node to get the position for.</param>
    /// <returns>The packed position of the node.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the node is unknown to the layout.</exception>
    public PackedPoint2D this[Point2D node]
    {
        get
        {
            var packed = new PackedPoint2D(
                Position: (node.X, node.Y / 2),
                Top: node.Y % 2 == 0);

            if (!_bounds.Contains(packed))
            {
                throw new KeyNotFoundException($"Node {node} is not in the layout.");
            }

            return packed;
        }
    }
}
