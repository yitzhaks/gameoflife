namespace GameOfLife.Core;

/// <summary>
/// Represents the result of a partial order comparison.
/// </summary>
public enum PartialOrdering
{
    /// <summary>
    /// The values are not comparable (neither less/equal nor greater/equal in all dimensions).
    /// </summary>
    Incomparable = 0,

    /// <summary>
    /// The first value is strictly less than the second in all dimensions.
    /// </summary>
    Less = -2,

    /// <summary>
    /// The first value is less than or equal in all dimensions, but not strictly less in all.
    /// </summary>
    LessOrEqual = -1,

    /// <summary>
    /// The first value is greater than or equal in all dimensions, but not strictly greater in all.
    /// </summary>
    GreaterOrEqual = 1,

    /// <summary>
    /// The first value is strictly greater than the second in all dimensions.
    /// </summary>
    Greater = 2
}

/// <summary>
/// Provides partial ordering comparisons for 2D points.
/// Unlike total ordering, partial ordering allows for incomparable elements.
/// </summary>
public static class PartialOrderingComparer
{
    /// <summary>
    /// Returns true if this point is strictly less than other in all dimensions.
    /// </summary>
    public static bool IsLess(this Point2D a, Point2D b) => a.X < b.X && a.Y < b.Y;

    /// <summary>
    /// Returns true if this point is less than or equal to other in all dimensions.
    /// </summary>
    public static bool IsLessOrEqual(this Point2D a, Point2D b) => a.X <= b.X && a.Y <= b.Y;

    /// <summary>
    /// Returns true if this point is strictly greater than other in all dimensions.
    /// </summary>
    public static bool IsGreater(this Point2D a, Point2D b) => a.X > b.X && a.Y > b.Y;

    /// <summary>
    /// Returns true if this point is greater than or equal to other in all dimensions.
    /// </summary>
    public static bool IsGreaterOrEqual(this Point2D a, Point2D b) => a.X >= b.X && a.Y >= b.Y;

    /// <summary>
    /// Returns true if this point is within bounds [0, size.Width) x [0, size.Height).
    /// </summary>
    public static bool IsInBounds(this Point2D point, Size2D size) =>
        point.IsGreaterOrEqual(default) &&
        point.IsLess((size.Width, size.Height));
}
