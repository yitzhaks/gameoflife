namespace GameOfLife.Core;

/// <summary>
/// Provides partial ordering comparisons for 2D points.
/// Unlike total ordering, partial ordering allows for incomparable elements.
/// </summary>
public static class PartialOrderingComparer
{
    /// <summary>
    /// Returns true if this point is strictly less than other in all dimensions.
    /// </summary>
    public static bool IsLessThan(this Point2D a, Point2D b) => a.X < b.X && a.Y < b.Y;

    /// <summary>
    /// Returns true if this point is less than or equal to other in all dimensions.
    /// </summary>
    public static bool IsLessThanOrEqualTo(this Point2D a, Point2D b) => a.X <= b.X && a.Y <= b.Y;

    /// <summary>
    /// Returns true if this point is strictly greater than other in all dimensions.
    /// </summary>
    public static bool IsGreaterThan(this Point2D a, Point2D b) => a.X > b.X && a.Y > b.Y;

    /// <summary>
    /// Returns true if this point is greater than or equal to other in all dimensions.
    /// </summary>
    public static bool IsGreaterThanOrEqualTo(this Point2D a, Point2D b) => a.X >= b.X && a.Y >= b.Y;

    /// <summary>
    /// Returns true if this point is within bounds [0, size.Width) x [0, size.Height).
    /// </summary>
    public static bool IsInBounds(this Point2D point, Size2D size) =>
        point.IsGreaterThanOrEqualTo(default) &&
        point.IsLessThan((size.Width, size.Height));
}
