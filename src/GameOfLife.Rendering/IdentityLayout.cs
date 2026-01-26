using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// A layout implementation for Point2D identities where the identity is used directly as the coordinate.
/// </summary>
public sealed class IdentityLayout : ILayout<Point2D, Point2D, RectangularBounds>
{
    private readonly ITopology<Point2D> _topology;
    private readonly IdentityLayoutPositions _positions;

    /// <summary>
    /// Gets the bounds of the layout region.
    /// </summary>
    public RectangularBounds Bounds { get; }

    /// <summary>
    /// Gets the positions of nodes in the layout.
    /// </summary>
    public ILayoutPositions<Point2D, Point2D> Positions => _positions;

    /// <summary>
    /// Creates a new identity layout for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <exception cref="ArgumentNullException">Thrown if topology is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the topology has no nodes.</exception>
    public IdentityLayout(ITopology<Point2D> topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        _topology = topology;

        // Compute bounds from the topology nodes
        var nodes = topology.Nodes.ToList();
        if (nodes.Count == 0)
        {
            throw new InvalidOperationException("Cannot create a layout for an empty topology.");
        }

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        foreach (var node in nodes)
        {
            if (node.X < minX)
            {
                minX = node.X;
            }

            if (node.Y < minY)
            {
                minY = node.Y;
            }

            if (node.X > maxX)
            {
                maxX = node.X;
            }

            if (node.Y > maxY)
            {
                maxY = node.Y;
            }
        }

        Bounds = new RectangularBounds(new Point2D(minX, minY), new Point2D(maxX, maxY));
        _positions = new IdentityLayoutPositions([.. nodes]);
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
        private readonly HashSet<Point2D> _validNodes;

        /// <summary>
        /// Creates a new identity layout positions instance.
        /// </summary>
        /// <param name="validNodes">The set of valid nodes in the topology.</param>
        public IdentityLayoutPositions(HashSet<Point2D> validNodes)
        {
            _validNodes = validNodes;
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
                if (!_validNodes.Contains(node))
                {
                    throw new KeyNotFoundException($"Node {node} is not in the layout.");
                }

                return node;
            }
        }
    }
}
