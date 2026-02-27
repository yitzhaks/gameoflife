using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// A layout implementation for hexagonal topologies mapping HexPoint to Point2D output coordinates.
/// Uses staggered layout where cells are offset to create the hexagonal pattern.
/// </summary>
public sealed class HexLayout : ILayout<HexPoint, Point2D, HexBounds>
{
    private readonly HexagonalTopology _topology;
    private readonly Dictionary<HexPoint, Point2D> _positions;

    /// <summary>
    /// Gets the bounds of the layout region.
    /// </summary>
    public HexBounds Bounds { get; }

    /// <summary>
    /// Gets the positions of nodes in the layout.
    /// </summary>
    public ILayoutPositions<HexPoint, Point2D> Positions { get; }

    /// <summary>
    /// Creates a new hex layout for the specified topology.
    /// </summary>
    /// <param name="topology">The topology to create a layout for.</param>
    /// <exception cref="ArgumentNullException">Thrown if topology is null.</exception>
    public HexLayout(HexagonalTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        _topology = topology;
        Bounds = HexBounds.ForHexLayout(topology.Radius);
        _positions = ComputePositions(topology);
        Positions = new HexLayoutPositions(_positions);
    }

    /// <summary>
    /// Enumerates nodes in the specified order.
    /// </summary>
    /// <param name="order">The comparer defining the enumeration order.</param>
    /// <returns>The nodes in the specified order.</returns>
    public IEnumerable<HexPoint> EnumerateNodes(IComparer<HexPoint> order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return _topology.Nodes.OrderBy(n => n, order);
    }

    private static Dictionary<HexPoint, Point2D> ComputePositions(HexagonalTopology topology)
    {
        var positions = new Dictionary<HexPoint, Point2D>();
        int radius = topology.Radius;

        foreach (HexPoint hex in topology.Nodes)
        {
            // Staggered layout:
            //       O   O   O        <- r = -2
            //     O   O   O   O      <- r = -1
            //   O   O   O   O   O    <- r = 0 (center row)
            //     O   O   O   O      <- r = 1
            //       O   O   O        <- r = 2
            //
            // Screen Y = r + radius (0-indexed from top)
            // Screen X = (q + radius) * 2 + offset
            //   where offset = |r| (indent for rows away from center)

            int y = hex.R + radius;
            int rowOffset = Math.Abs(hex.R);
            int x = ((hex.Q + radius) * 2) + rowOffset;

            positions[hex] = (x, y);
        }

        return positions;
    }

    /// <summary>
    /// Provides position lookup for hex layouts.
    /// </summary>
    private sealed class HexLayoutPositions : ILayoutPositions<HexPoint, Point2D>
    {
        private readonly Dictionary<HexPoint, Point2D> _positions;

        public HexLayoutPositions(Dictionary<HexPoint, Point2D> positions)
        {
            _positions = positions;
        }

        public Point2D this[HexPoint node]
        {
            get
            {
                if (!_positions.TryGetValue(node, out Point2D position))
                {
                    throw new KeyNotFoundException($"Node {node} is not in the layout.");
                }

                return position;
            }
        }
    }
}
