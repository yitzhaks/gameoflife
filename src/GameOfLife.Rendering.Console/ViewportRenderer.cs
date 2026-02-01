namespace GameOfLife.Rendering.Console;

/// <summary>
/// Encapsulates viewport clipping logic and exposes edge state for border rendering.
/// </summary>
/// <remarks>
/// This class handles the calculation of render bounds based on an optional viewport
/// and provides properties indicating whether the viewport is at each edge of the board.
/// Used by <see cref="BorderedRenderer"/> to determine border appearance.
/// </remarks>
internal sealed class ViewportRenderer
{
    private readonly Viewport? _viewport;

    /// <summary>
    /// Creates a new viewport renderer for rectangular (identity) layout bounds.
    /// </summary>
    /// <param name="bounds">The bounds from the layout.</param>
    /// <param name="viewport">Optional viewport for clipping. If null, renders the full bounds.</param>
    public ViewportRenderer(RectangularBounds bounds, Viewport? viewport)
    {
        _viewport = viewport;

        if (viewport is not null)
        {
            RenderStartX = bounds.Min.X + viewport.OffsetX;
            RenderStartY = bounds.Min.Y + viewport.OffsetY;
            RenderEndX = Math.Min(RenderStartX + viewport.Width - 1, bounds.Max.X);
            RenderEndY = Math.Min(RenderStartY + viewport.Height - 1, bounds.Max.Y);
            Width = RenderEndX - RenderStartX + 1;
        }
        else
        {
            RenderStartX = bounds.Min.X;
            RenderStartY = bounds.Min.Y;
            RenderEndX = bounds.Max.X;
            RenderEndY = bounds.Max.Y;
            Width = bounds.Max.X - bounds.Min.X + 1;
        }
    }

    /// <summary>
    /// Creates a new viewport renderer for packed (half-block) layout bounds.
    /// </summary>
    /// <param name="bounds">The packed bounds from the half-block layout.</param>
    /// <param name="viewport">Optional viewport for clipping. If null, renders the full bounds.</param>
    public ViewportRenderer(PackedBounds bounds, Viewport? viewport)
    {
        _viewport = viewport;

        if (viewport is not null)
        {
            RenderStartX = viewport.OffsetX;
            RenderStartY = viewport.OffsetY;
            RenderEndX = Math.Min(RenderStartX + viewport.Width - 1, bounds.Max.X);
            RenderEndY = Math.Min(RenderStartY + viewport.Height - 1, bounds.Max.Y);
            Width = RenderEndX - RenderStartX + 1;
        }
        else
        {
            RenderStartX = bounds.Min.X;
            RenderStartY = bounds.Min.Y;
            RenderEndX = bounds.Max.X;
            RenderEndY = bounds.Max.Y;
            Width = bounds.Width;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the viewport is at the top edge of the board.
    /// </summary>
    /// <remarks>
    /// Returns true if there is no viewport (full board is shown) or if the viewport is at the top.
    /// </remarks>
    public bool IsAtTop => _viewport?.IsAtTop ?? true;

    /// <summary>
    /// Gets a value indicating whether the viewport is at the bottom edge of the board.
    /// </summary>
    /// <remarks>
    /// Returns true if there is no viewport (full board is shown) or if the viewport is at the bottom.
    /// </remarks>
    public bool IsAtBottom => _viewport?.IsAtBottom ?? true;

    /// <summary>
    /// Gets a value indicating whether the viewport is at the left edge of the board.
    /// </summary>
    /// <remarks>
    /// Returns true if there is no viewport (full board is shown) or if the viewport is at the left.
    /// </remarks>
    public bool IsAtLeft => _viewport?.IsAtLeft ?? true;

    /// <summary>
    /// Gets a value indicating whether the viewport is at the right edge of the board.
    /// </summary>
    /// <remarks>
    /// Returns true if there is no viewport (full board is shown) or if the viewport is at the right.
    /// </remarks>
    public bool IsAtRight => _viewport?.IsAtRight ?? true;

    /// <summary>
    /// Gets the starting X coordinate for rendering (in board/packed coordinates).
    /// </summary>
    public int RenderStartX { get; }

    /// <summary>
    /// Gets the starting Y coordinate for rendering (in board/packed coordinates).
    /// </summary>
    public int RenderStartY { get; }

    /// <summary>
    /// Gets the ending X coordinate for rendering, inclusive (in board/packed coordinates).
    /// </summary>
    public int RenderEndX { get; }

    /// <summary>
    /// Gets the ending Y coordinate for rendering, inclusive (in board/packed coordinates).
    /// </summary>
    public int RenderEndY { get; }

    /// <summary>
    /// Gets the width of the rendered area in cells.
    /// </summary>
    public int Width { get; }
}
