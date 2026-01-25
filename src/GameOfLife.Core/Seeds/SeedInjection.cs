using GameOfLife.Core;

namespace GameOfLife.Core.Seeds;

public static class SeedInjection
{
    public static IReadOnlySet<Point2D> Apply(SeedPattern seed, Point2D offset, int gridWidth, int gridHeight)
    {
        ArgumentNullException.ThrowIfNull(seed);

        if (gridWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gridWidth), "Grid width must be positive.");
        }

        if (gridHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gridHeight), "Grid height must be positive.");
        }

        var injected = new HashSet<Point2D>();
        foreach (var point in seed.AlivePoints)
        {
            var target = new Point2D(point.X + offset.X, point.Y + offset.Y);
            if (target.X < 0 || target.X >= gridWidth || target.Y < 0 || target.Y >= gridHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), $"Seed point {target} is out of bounds.");
            }

            injected.Add(target);
        }

        return injected;
    }

    public static bool[,] ApplyToGrid(SeedPattern seed, Point2D offset, int gridWidth, int gridHeight)
    {
        var alive = Apply(seed, offset, gridWidth, gridHeight);
        var grid = new bool[gridHeight, gridWidth];
        foreach (var point in alive)
        {
            grid[point.Y, point.X] = true;
        }

        return grid;
    }
}
