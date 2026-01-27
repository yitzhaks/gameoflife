namespace GameOfLife.Core;

/// <summary>
/// A finite 2D rectangular grid topology with Moore neighborhood (8 neighbors).
/// </summary>
public class RectangularTopology : ITopology<Point2D>
{
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
