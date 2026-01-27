namespace GameOfLife.Core;

/// <summary>
/// Represents a 2D coordinate used to identify nodes in a 2D topology.
/// </summary>
/// <param name="X">The X coordinate.</param>
/// <param name="Y">The Y coordinate.</param>
public readonly record struct Point2D(int X, int Y)
{
    /// <summary>
    /// Implicitly converts a tuple to a Point2D.
    /// </summary>
    /// <param name="tuple">The tuple containing X and Y coordinates.</param>
    public static implicit operator Point2D((int X, int Y) tuple) => new(tuple.X, tuple.Y);

    /// <summary>
    /// Adds two points component-wise.
    /// </summary>
    public static Point2D operator +(Point2D a, Point2D b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtracts the second point from the first component-wise.
    /// </summary>
    public static Point2D operator -(Point2D a, Point2D b) => new(a.X - b.X, a.Y - b.Y);
}
