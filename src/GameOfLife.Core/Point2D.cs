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
}
