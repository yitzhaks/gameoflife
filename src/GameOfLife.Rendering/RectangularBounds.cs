using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// Represents axis-aligned rectangular bounds in 2D space.
/// Uses inclusive min and max corners.
/// </summary>
public sealed class RectangularBounds : IAxisAlignedBounds<Point2D>
{
    /// <summary>
    /// Gets the minimum corner of the bounds (inclusive).
    /// </summary>
    public Point2D Min { get; }

    /// <summary>
    /// Gets the maximum corner of the bounds (inclusive).
    /// </summary>
    public Point2D Max { get; }

    /// <summary>
    /// Creates a new rectangular bounds with the specified corners.
    /// </summary>
    /// <param name="min">The minimum corner (inclusive).</param>
    /// <param name="max">The maximum corner (inclusive).</param>
    /// <exception cref="ArgumentException">Thrown if min is greater than max in any dimension.</exception>
    public RectangularBounds(Point2D min, Point2D max)
    {
        if (min.X > max.X)
        {
            throw new ArgumentException($"Min X ({min.X}) cannot be greater than Max X ({max.X}).", nameof(min));
        }

        if (min.Y > max.Y)
        {
            throw new ArgumentException($"Min Y ({min.Y}) cannot be greater than Max Y ({max.Y}).", nameof(min));
        }

        Min = min;
        Max = max;
    }

    /// <summary>
    /// Determines whether the specified coordinate is within the bounds.
    /// </summary>
    /// <param name="coordinate">The coordinate to test.</param>
    /// <returns><c>true</c> if the coordinate is within the bounds; otherwise, <c>false</c>.</returns>
    public bool Contains(Point2D coordinate)
    {
        return coordinate.X >= Min.X && coordinate.X <= Max.X
            && coordinate.Y >= Min.Y && coordinate.Y <= Max.Y;
    }
}
