using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// Represents axis-aligned bounds for hexagonal layouts in output (Point2D) coordinate space.
/// </summary>
public sealed class HexBounds : IAxisAlignedBounds<Point2D>
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
    /// Creates bounds for a hexagonal layout with the specified corners.
    /// </summary>
    /// <param name="min">The minimum corner (inclusive).</param>
    /// <param name="max">The maximum corner (inclusive).</param>
    /// <exception cref="ArgumentException">Thrown if min is greater than max in any dimension.</exception>
    public HexBounds(Point2D min, Point2D max)
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
    /// Creates bounds for a hexagonal layout based on radius.
    /// Uses staggered mode where each row has different width based on radius.
    /// </summary>
    /// <param name="hexRadius">The radius of the hexagonal topology.</param>
    public static HexBounds ForHexLayout(int hexRadius)
    {
        // Staggered mode: Each row has different width based on radius
        // Height: (2 * radius + 1) rows
        // Width: (2 * radius + 1) columns at widest, with stagger offset
        // The widest row (r=0) has (2*radius + 1) cells, each 2 chars wide
        int height = (2 * hexRadius) + 1;
        int width = ((2 * hexRadius) + 1) * 2; // 2 chars per cell

        return new HexBounds(default, (width - 1, height - 1));
    }

    /// <summary>
    /// Determines whether the specified coordinate is within the bounds.
    /// </summary>
    /// <param name="coordinate">The coordinate to test.</param>
    /// <returns>True if the coordinate is within the bounds; otherwise, false.</returns>
    public bool Contains(Point2D coordinate) =>
        coordinate.X >= Min.X && coordinate.X <= Max.X &&
        coordinate.Y >= Min.Y && coordinate.Y <= Max.Y;
}
