using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// A layout implementation for RectangularTopology where the identity is used directly as the coordinate.
/// </summary>
public sealed class IdentityLayout : ILayout<Point2D, Point2D, RectangularBounds>
{
    private readonly RectangularTopology _topology;

    /// <summary>
    /// Gets the bounds of the layout region.
    /// </summary>
    public RectangularBounds Bounds => new(_topology.Size);

    /// <summary>
    /// Gets the positions of nodes in the layout.
    /// </summary>
    public ILayoutPositions<Point2D, Point2D> Positions { get; }

    /// <summary>
    /// Creates a new identity layout for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <exception cref="ArgumentNullException">Thrown if topology is null.</exception>
    public IdentityLayout(RectangularTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        _topology = topology;
        Positions = new IdentityLayoutPositions(Bounds);
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

    /// <summary>
    /// Provides position lookup for identity-based layouts.
    /// </summary>
    private sealed class IdentityLayoutPositions : ILayoutPositions<Point2D, Point2D>
    {
        private readonly RectangularBounds _bounds;

        /// <summary>
        /// Creates a new identity layout positions instance.
        /// </summary>
        /// <param name="bounds">The bounds defining valid positions.</param>
        public IdentityLayoutPositions(RectangularBounds bounds)
        {
            _bounds = bounds;
        }

        /// <summary>
        /// Gets the layout position of the specified node.
        /// For identity layouts, the position equals the identity.
        /// </summary>
        /// <param name="node">The node to get the position for.</param>
        /// <returns>The node itself as its position.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the node is unknown to the layout.</exception>
        public Point2D this[Point2D node]
        {
            get
            {
                if (!_bounds.Contains(node))
                {
                    throw new KeyNotFoundException($"Node {node} is not in the layout.");
                }

                return node;
            }
        }
    }
}
