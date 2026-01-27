namespace GameOfLife.Core;

/// <summary>
/// Represents a 2D size (width and height).
/// </summary>
/// <param name="Width">The width.</param>
/// <param name="Height">The height.</param>
public readonly record struct Size2D(int Width, int Height)
{
    /// <summary>
    /// Implicitly converts a tuple to a Size2D.
    /// </summary>
    /// <param name="tuple">The tuple containing Width and Height.</param>
    public static implicit operator Size2D((int Width, int Height) tuple) => new(tuple.Width, tuple.Height);
}
