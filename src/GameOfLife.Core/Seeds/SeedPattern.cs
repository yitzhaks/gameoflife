using GameOfLife.Core;

namespace GameOfLife.Core.Seeds;

public sealed class SeedPattern
{
    public int Width { get; }

    public int Height { get; }

    public IReadOnlySet<Point2D> AlivePoints { get; }

    public SeedPattern(int width, int height, IEnumerable<Point2D> alivePoints)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        ArgumentNullException.ThrowIfNull(alivePoints);

        var points = new HashSet<Point2D>();
        foreach (var point in alivePoints)
        {
            if (point.X < 0 || point.X >= width)
            {
                throw new ArgumentOutOfRangeException(nameof(alivePoints), $"Seed point {point} is out of bounds.");
            }

            if (point.Y < 0 || point.Y >= height)
            {
                throw new ArgumentOutOfRangeException(nameof(alivePoints), $"Seed point {point} is out of bounds.");
            }

            points.Add(point);
        }

        Width = width;
        Height = height;
        AlivePoints = points;
    }
}
