namespace GameOfLife.Core.Tests;

/// <summary>
/// Helper methods for creating test fixtures.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Creates an empty generation with all cells dead.
    /// </summary>
    public static IGeneration<Point2D, bool> CreateEmptyGeneration(Size2D size)
    {
        using var builder = new RectangularGenerationBuilder(size);
        // Build() takes ownership of the pooled array
        return builder.Build();
    }

    /// <summary>
    /// Creates a generation with the specified initial states.
    /// Out-of-bounds coordinates are silently ignored.
    /// </summary>
    public static IGeneration<Point2D, bool> CreateGeneration(Size2D size, IReadOnlyDictionary<Point2D, bool> states)
    {
        using var builder = new RectangularGenerationBuilder(size);
        foreach ((Point2D point, bool state) in states)
        {
            if (state && point.IsInBounds(size))
            {
                builder[point] = true;
            }
        }

        // Build() takes ownership of the pooled array
        return builder.Build();
    }
}
