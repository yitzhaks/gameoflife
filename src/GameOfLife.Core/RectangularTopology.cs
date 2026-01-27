namespace GameOfLife.Core;

/// <summary>
/// A finite 2D rectangular grid topology with Moore neighborhood (8 neighbors).
/// </summary>
public class RectangularTopology : ITopology<Point2D>
{
    /// <summary>
    /// Stack-allocated enumerable for neighbors. Use with foreach for zero-allocation iteration.
    /// </summary>
    public readonly ref struct NeighborEnumerable
    {
        private readonly Point2D _center;
        private readonly Size2D _size;

        internal NeighborEnumerable(Point2D center, Size2D size)
        {
            _center = center;
            _size = size;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public NeighborEnumerator GetEnumerator() => new(_center, _size);
    }

    /// <summary>
    /// Stack-allocated enumerator for neighbors. Iterates Moore neighborhood without heap allocation.
    /// </summary>
    public ref struct NeighborEnumerator
    {
        private readonly Point2D _center;
        private readonly Size2D _size;
        private int _index;

        internal NeighborEnumerator(Point2D center, Size2D size)
        {
            _center = center;
            _size = size;
            _index = -1;
            Current = default;
        }

        /// <summary>
        /// Gets the current neighbor.
        /// </summary>
        public Point2D Current { get; private set; }

        /// <summary>
        /// Moves to the next neighbor.
        /// </summary>
        public bool MoveNext()
        {
            // Offsets for Moore neighborhood (8 directions), skipping (0,0)
            ReadOnlySpan<int> dx = [-1, 0, 1, -1, 1, -1, 0, 1];
            ReadOnlySpan<int> dy = [-1, -1, -1, 0, 0, 1, 1, 1];

            while (++_index < 8)
            {
                Point2D neighbor = _center + (dx[_index], dy[_index]);
                if (neighbor.IsInBounds(_size))
                {
                    Current = neighbor;
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Gets the size of the grid.
    /// </summary>
    public Size2D Size { get; }

    /// <summary>
    /// Creates a new RectangularTopology with the specified dimensions.
    /// </summary>
    /// <param name="size">The size of the grid.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if width or height is less than 1.</exception>
    public RectangularTopology(Size2D size)
    {
        if (size.Width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Width must be at least 1.");
        }

        if (size.Height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Height must be at least 1.");
        }

        Size = size;
    }

    /// <summary>
    /// Gets all nodes in the grid from (0,0) to (width-1, height-1).
    /// </summary>
    public IEnumerable<Point2D> Nodes
    {
        get
        {
            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    yield return (x, y);
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
    public NeighborEnumerable GetNeighborsStack(Point2D node) => new(node, Size);

    /// <summary>
    /// Gets the neighbors of a node (Moore neighborhood - up to 8 adjacent cells).
    /// </summary>
    /// <param name="node">The node to get neighbors for.</param>
    /// <returns>The neighboring nodes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the node is outside the grid.</exception>
    public IEnumerable<Point2D> GetNeighbors(Point2D node)
    {
        if (!node.IsInBounds(Size))
        {
            throw new ArgumentOutOfRangeException(nameof(node), "Node is outside the grid boundaries.");
        }

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                Point2D neighbor = node + (dx, dy);
                if (neighbor.IsInBounds(Size))
                {
                    yield return neighbor;
                }
            }
        }
    }
}
