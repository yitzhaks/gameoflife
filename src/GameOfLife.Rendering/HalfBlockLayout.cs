using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// A layout implementation for RectangularTopology that packs two vertically-adjacent cells
/// into a single output position for half-block rendering.
/// </summary>
public sealed class HalfBlockLayout : ILayout<Point2D, PackedPoint2D, PackedBounds>
{
    private readonly RectangularTopology _topology;

    /// <summary>
    /// Gets the bounds of the layout region in packed space.
    /// </summary>
    public PackedBounds Bounds { get; }

    /// <summary>
    /// Gets the positions of nodes in the layout.
    /// </summary>
    public ILayoutPositions<Point2D, PackedPoint2D> Positions { get; }

    /// <summary>
    /// Creates a new half-block layout for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <exception cref="ArgumentNullException">Thrown if topology is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the topology height is odd.</exception>
    public HalfBlockLayout(RectangularTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        if (topology.Size.Height % 2 != 0)
        {
            throw new ArgumentException(
                $"Half-block mode requires even height, but got {topology.Size.Height}. " +
                "Each output row represents exactly 2 input rows.",
                nameof(topology));
        }

        _topology = topology;
        Bounds = new PackedBounds(topology.Size.Width, topology.Size.Height);
        Positions = new HalfBlockLayoutPositions(Bounds);
    }

    /// <summary>
    /// Enumerates nodes in the specified order.
    /// </summary>
    /// <param name="order">The comparer defining the enumeration order.</param>
    /// <returns>The nodes in the specified order.</returns>
    public IEnumerable<Point2D> EnumerateNodes(IComparer<Point2D> order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return _topology.Nodes.OrderBy(n => n, order);
    }
}
