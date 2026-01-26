using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// A layout implementation for Grid2DTopology where the identity is used directly as the coordinate.
/// </summary>
public sealed class IdentityLayout : ILayout<Point2D, Point2D, RectangularBounds>
{
    private readonly Grid2DTopology _topology;

    /// <summary>
    /// Gets the bounds of the layout region.
    /// </summary>
    public RectangularBounds Bounds { get; }

    /// <summary>
    /// Gets the positions of nodes in the layout.
    /// </summary>
    public ILayoutPositions<Point2D, Point2D> Positions { get; }

    /// <summary>
    /// Creates a new identity layout for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <exception cref="ArgumentNullException">Thrown if topology is null.</exception>
    public IdentityLayout(Grid2DTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        _topology = topology;

        // Compute bounds directly from grid dimensions - O(1)
        Bounds = new RectangularBounds(new Point2D(0, 0), new Point2D(topology.Width - 1, topology.Height - 1));
        Positions = new IdentityLayoutPositions(topology);
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
        private readonly Grid2DTopology _topology;

        /// <summary>
        /// Creates a new identity layout positions instance.
        /// </summary>
        /// <param name="topology">The grid topology defining valid nodes.</param>
        public IdentityLayoutPositions(Grid2DTopology topology)
        {
            _topology = topology;
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
                // O(1) bounds check using grid dimensions
                if (node.X < 0 || node.X >= _topology.Width || node.Y < 0 || node.Y >= _topology.Height)
                {
                    throw new KeyNotFoundException($"Node {node} is not in the layout.");
                }

                return node;
            }
        }
    }
}
