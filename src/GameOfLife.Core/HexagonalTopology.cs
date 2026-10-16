namespace GameOfLife.Core;

/// <summary>
/// A finite hexagonal topology with 6-neighbor connectivity.
/// The topology is defined by a radius, including all cells where max(|q|, |r|, |s|) &lt;= radius.
/// </summary>
public class HexagonalTopology : ITopology<HexPoint>
{
    /// <summary>
    /// The 6 axial direction offsets for hex neighbors.
    /// </summary>
    private static readonly HexPoint[] s_neighborOffsets =
    [
        (1, 0),   // East
        (-1, 0),  // West
        (1, -1),  // Northeast
        (0, -1),  // Northwest
        (0, 1),   // Southeast
        (-1, 1)   // Southwest
    ];

    /// <summary>
    /// Stack-allocated enumerable for neighbors. Use with foreach for zero-allocation iteration.
    /// </summary>
    public readonly ref struct NeighborEnumerable
    {
        private readonly HexPoint _center;
        private readonly int _radius;

        internal NeighborEnumerable(HexPoint center, int radius)
        {
            _center = center;
            _radius = radius;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public NeighborEnumerator GetEnumerator() => new(_center, _radius);
    }

    /// <summary>
    /// Stack-allocated enumerator for neighbors. Iterates hex neighborhood without heap allocation.
    /// </summary>
    public ref struct NeighborEnumerator
    {
        private readonly HexPoint _center;
        private readonly int _radius;
        private int _index;

        internal NeighborEnumerator(HexPoint center, int radius)
        {
            _center = center;
            _radius = radius;
            _index = -1;
            Current = default;
        }

        /// <summary>
        /// Gets the current neighbor.
        /// </summary>
        public HexPoint Current { get; private set; }

        /// <summary>
        /// Moves to the next neighbor.
        /// </summary>
        public bool MoveNext()
        {
            while (++_index < 6)
            {
                HexPoint neighbor = _center + s_neighborOffsets[_index];
                if (neighbor.IsWithinRadius(_radius))
                {
                    Current = neighbor;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Gets the radius of the hexagonal topology.
    /// </summary>
    public int Radius { get; }

    /// <summary>
    /// Gets the total number of cells in the topology.
    /// Formula: 3 * r * (r + 1) + 1
    /// </summary>
    public int CellCount { get; }

    /// <summary>
    /// Creates a new hexagonal topology with the specified radius.
    /// </summary>
    /// <param name="radius">The radius of the hex board (0 = single cell, 1 = 7 cells, etc.).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if radius is negative.</exception>
    public HexagonalTopology(int radius)
    {
        if (radius < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be non-negative.");
        }

        Radius = radius;
        CellCount = (3 * radius * (radius + 1)) + 1;
    }

    /// <summary>
    /// Gets all nodes in the hexagonal topology.
    /// Iterates in a consistent order from top-left to bottom-right.
    /// </summary>
    public IEnumerable<HexPoint> Nodes
    {
        get
        {
            for (int r = -Radius; r <= Radius; r++)
            {
                int qMin = Math.Max(-Radius, -r - Radius);
                int qMax = Math.Min(Radius, -r + Radius);
                for (int q = qMin; q <= qMax; q++)
                {
                    yield return (q, r);
                }
            }
        }
    }

    /// <summary>
    /// Gets the neighbors of a node without heap allocation.
    /// Use this in hot paths with foreach for zero-allocation iteration.
    /// </summary>
    /// <param name="node">The node to get neighbors for.</param>
    /// <returns>A stack-allocated enumerable of neighboring nodes.</returns>
    public NeighborEnumerable GetNeighborsStack(HexPoint node) => new(node, Radius);

    /// <summary>
    /// Gets the neighbors of a node (6-connectivity for hexagonal grids).
    /// </summary>
    /// <param name="node">The node to get neighbors for.</param>
    /// <returns>The neighboring nodes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the node is outside the topology.</exception>
    public IEnumerable<HexPoint> GetNeighbors(HexPoint node)
    {
        if (!node.IsWithinRadius(Radius))
        {
            throw new ArgumentOutOfRangeException(nameof(node), "Node is outside the hexagonal topology boundaries.");
        }

        return s_neighborOffsets
            .Select(offset => node + offset)
            .Where(neighbor => neighbor.IsWithinRadius(Radius));
    }

    /// <summary>
    /// Checks whether the given node is within this topology.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>True if the node is within the topology; otherwise, false.</returns>
    public bool Contains(HexPoint node) => node.IsWithinRadius(Radius);
}
