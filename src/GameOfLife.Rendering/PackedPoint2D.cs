using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// Represents a position in packed coordinate space for half-block rendering.
/// Each PackedPoint2D corresponds to two vertically-adjacent cells.
/// </summary>
/// <param name="Position">The position in packed space (Y is halved).</param>
/// <param name="Top">True if this represents the top cell of the pair, false for the bottom cell.</param>
public readonly record struct PackedPoint2D(Point2D Position, bool Top) : IEquatable<PackedPoint2D>
{
    /// <summary>
    /// Gets the X coordinate.
    /// </summary>
    public int X => Position.X;

    /// <summary>
    /// Gets the Y coordinate in packed space.
    /// </summary>
    public int Y => Position.Y;

    /// <summary>
    /// Creates a PackedPoint2D from a Point2D in original coordinate space.
    /// </summary>
    /// <param name="original">The original point.</param>
    /// <returns>The packed point with Top indicating whether the original Y was even.</returns>
    public static PackedPoint2D FromOriginal(Point2D original) =>
        new((original.X, original.Y / 2), original.Y % 2 == 0);

    /// <summary>
    /// Gets the original Point2D that this packed point represents.
    /// </summary>
    /// <returns>The original point in uncompressed coordinate space.</returns>
    public Point2D ToOriginal() => (Position.X, (Position.Y * 2) + (Top ? 0 : 1));
}
