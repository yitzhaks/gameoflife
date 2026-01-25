namespace GameOfLife.Core;

public sealed class DenseGeneration : IGeneration<Point2D, bool>
{
    private readonly bool[] _cells;
    private readonly int _width;
    private readonly int _height;

    public DenseGeneration(int width, int height, IEnumerable<Point2D>? aliveCells = null)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        _width = width;
        _height = height;
        _cells = new bool[width * height];

        if (aliveCells is null)
        {
            return;
        }

        foreach (var cell in aliveCells)
        {
            _cells[GetIndex(cell)] = true;
        }
    }

    public DenseGeneration(int width, int height, IReadOnlyCollection<Point2D> aliveCells)
        : this(width, height, aliveCells.AsEnumerable())
    {
    }

    public bool this[Point2D node]
    {
        get => _cells[GetIndex(node)];
    }

    private int GetIndex(Point2D node)
    {
        if (node.X < 0 || node.X >= _width || node.Y < 0 || node.Y >= _height)
        {
            throw new ArgumentOutOfRangeException(nameof(node), "Node is not part of this generation.");
        }

        return node.Y * _width + node.X;
    }
}
