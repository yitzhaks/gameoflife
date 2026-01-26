namespace GameOfLife.Core;

/// <summary>
/// A finite 2D rectangular grid topology with Moore neighborhood (8 neighbors).
/// </summary>
public class Grid2DTopology : ITopology<Point2D>
{
    private readonly int _width;
    private readonly int _height;

    /// <summary>
    /// Creates a new Grid2DTopology with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the grid.</param>
    /// <param name="height">The height of the grid.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if width or height is less than 1.</exception>
    public Grid2DTopology(int width, int height)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be at least 1.");
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be at least 1.");
        }

        _width = width;
        _height = height;
    }

    /// <summary>
    /// Gets all nodes in the grid from (0,0) to (width-1, height-1).
    /// </summary>
    public IEnumerable<Point2D> Nodes
    {
        get
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    yield return new Point2D(x, y);
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
        if (node.X < 0 || node.X >= _width || node.Y < 0 || node.Y >= _height)
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

                var nx = node.X + dx;
                var ny = node.Y + dy;

                if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                {
                    yield return new Point2D(nx, ny);
                }
            }
        }
    }
}
