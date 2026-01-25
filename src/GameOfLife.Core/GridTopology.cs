namespace GameOfLife.Core;

public sealed class GridTopology : ITopology<Point2D>
{
    private readonly Point2D[] _nodes;
    private readonly Dictionary<Point2D, int> _indexByNode;

    public GridTopology(int width, int height)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        Width = width;
        Height = height;
        _nodes = new Point2D[width * height];
        _indexByNode = new Dictionary<Point2D, int>(_nodes.Length);

        var index = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var node = new Point2D(x, y);
                _nodes[index] = node;
                _indexByNode[node] = index;
                index++;
            }
        }
    }

    public int Width { get; }
    public int Height { get; }

    public IEnumerable<Point2D> Nodes => _nodes;

    public IEnumerable<Point2D> GetNeighbors(Point2D node)
    {
        if (!_indexByNode.ContainsKey(node))
        {
            throw new ArgumentOutOfRangeException(nameof(node), "Node is not part of this topology.");
        }

        for (var dy = -1; dy <= 1; dy++)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                var x = node.X + dx;
                var y = node.Y + dy;
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                {
                    continue;
                }

                yield return new Point2D(x, y);
            }
        }
    }
}
