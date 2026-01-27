namespace GameOfLife.Rendering;

/// <summary>
/// Represents axis-aligned bounds in packed coordinate space for half-block rendering.
/// Uses inclusive min and max corners.
/// </summary>
public sealed class PackedBounds : IAxisAlignedBounds<PackedPoint2D>
{
    /// <summary>
    /// Gets the minimum corner of the bounds (inclusive).
    /// </summary>
    public PackedPoint2D Min { get; }

    /// <summary>
    /// Gets the maximum corner of the bounds (inclusive).
    /// </summary>
    public PackedPoint2D Max { get; }

    /// <summary>
    /// Gets the width of the bounds.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the bounds in packed space.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the original height before packing (always even).
    /// </summary>
    public int OriginalHeight { get; }

    /// <summary>
    /// Creates packed bounds from original dimensions.
    /// </summary>
    /// <param name="width">The width (unchanged by packing).</param>
    /// <param name="originalHeight">The original height (must be even or zero).</param>
    /// <exception cref="ArgumentException">Thrown if originalHeight is odd.</exception>
    public PackedBounds(int width, int originalHeight)
    {
        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width cannot be negative.");
        }

        if (originalHeight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(originalHeight), "Height cannot be negative.");
        }

        if (originalHeight % 2 != 0)
        {
            throw new ArgumentException(
                $"Half-block mode requires even height, but got {originalHeight}. " +
                "Each output row represents exactly 2 input rows.",
                nameof(originalHeight));
        }

        Width = width;
        OriginalHeight = originalHeight;
        Height = originalHeight / 2;

        // For empty bounds (0 width or 0 height), set min/max appropriately
        if (width == 0 || originalHeight == 0)
        {
            Min = new PackedPoint2D((0, 0), Top: true);
            Max = new PackedPoint2D((-1, -1), Top: false);
        }
        else
        {
            Min = new PackedPoint2D((0, 0), Top: true);
            Max = new PackedPoint2D((width - 1, Height - 1), Top: false);
        }
    }

    /// <summary>
    /// Determines whether the specified coordinate is within the bounds.
    /// </summary>
    /// <param name="coordinate">The coordinate to test.</param>
    /// <returns><c>true</c> if the coordinate is within the bounds; otherwise, <c>false</c>.</returns>
    public bool Contains(PackedPoint2D coordinate)
    {
        if (Width == 0 || Height == 0)
        {
            return false;
        }

        return coordinate.X >= 0 && coordinate.X < Width &&
               coordinate.Y >= 0 && coordinate.Y < Height;
    }
}
