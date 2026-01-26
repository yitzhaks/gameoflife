namespace GameOfLife.Rendering.Console;

/// <summary>
/// Represents a viewport into a larger board, defining visible bounds and position.
/// </summary>
public sealed class Viewport
{
    /// <summary>
    /// Gets the horizontal offset of the viewport (left edge position in board coordinates).
    /// </summary>
    public int OffsetX { get; private set; }

    /// <summary>
    /// Gets the vertical offset of the viewport (top edge position in board coordinates).
    /// </summary>
    public int OffsetY { get; private set; }

    /// <summary>
    /// Gets the width of the viewport (visible columns).
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the viewport (visible rows).
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the total width of the board.
    /// </summary>
    public int BoardWidth { get; }

    /// <summary>
    /// Gets the total height of the board.
    /// </summary>
    public int BoardHeight { get; }

    /// <summary>
    /// Gets a value indicating whether the viewport is at the top edge of the board.
    /// </summary>
    public bool IsAtTop => OffsetY == 0;

    /// <summary>
    /// Gets a value indicating whether the viewport is at the bottom edge of the board.
    /// </summary>
    public bool IsAtBottom => OffsetY + Height >= BoardHeight;

    /// <summary>
    /// Gets a value indicating whether the viewport is at the left edge of the board.
    /// </summary>
    public bool IsAtLeft => OffsetX == 0;

    /// <summary>
    /// Gets a value indicating whether the viewport is at the right edge of the board.
    /// </summary>
    public bool IsAtRight => OffsetX + Width >= BoardWidth;

    /// <summary>
    /// Creates a new viewport.
    /// </summary>
    /// <param name="viewportWidth">The width of the viewport.</param>
    /// <param name="viewportHeight">The height of the viewport.</param>
    /// <param name="boardWidth">The total width of the board.</param>
    /// <param name="boardHeight">The total height of the board.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if any dimension is less than or equal to zero.
    /// </exception>
    public Viewport(int viewportWidth, int viewportHeight, int boardWidth, int boardHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(viewportWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(viewportHeight);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boardWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boardHeight);

        Width = viewportWidth;
        Height = viewportHeight;
        BoardWidth = boardWidth;
        BoardHeight = boardHeight;
        OffsetX = 0;
        OffsetY = 0;
    }

    /// <summary>
    /// Moves the viewport by the specified delta, clamping to board bounds.
    /// </summary>
    /// <param name="deltaX">The horizontal movement (positive = right, negative = left).</param>
    /// <param name="deltaY">The vertical movement (positive = down, negative = up).</param>
    public void Move(int deltaX, int deltaY)
    {
        int newOffsetX = OffsetX + deltaX;
        int newOffsetY = OffsetY + deltaY;

        // Clamp to valid range
        int maxOffsetX = Math.Max(0, BoardWidth - Width);
        int maxOffsetY = Math.Max(0, BoardHeight - Height);

        OffsetX = Math.Clamp(newOffsetX, 0, maxOffsetX);
        OffsetY = Math.Clamp(newOffsetY, 0, maxOffsetY);
    }

    /// <summary>
    /// Determines whether the specified board coordinate is visible within the viewport.
    /// </summary>
    /// <param name="boardX">The X coordinate in board space.</param>
    /// <param name="boardY">The Y coordinate in board space.</param>
    /// <returns>True if the coordinate is visible; otherwise, false.</returns>
    public bool Contains(int boardX, int boardY)
    {
        return boardX >= OffsetX && boardX < OffsetX + Width &&
               boardY >= OffsetY && boardY < OffsetY + Height;
    }
}
