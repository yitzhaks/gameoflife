namespace GameOfLife.Rendering.Console;

/// <summary>
/// Static helper methods for border rendering logic.
/// </summary>
/// <remarks>
/// This class provides shared utilities for determining border characters and colors
/// based on viewport edge state. Used by <see cref="BorderedRenderer"/> to eliminate
/// duplication between <see cref="TokenEnumerator"/> and <see cref="HalfBlockTokenEnumerator"/>.
/// </remarks>
internal static class BorderHelper
{
    /// <summary>
    /// Gets the character for the top-left corner based on viewport edge state.
    /// </summary>
    /// <param name="isAtTop">Whether the viewport is at the top edge of the board.</param>
    /// <param name="isAtLeft">Whether the viewport is at the left edge of the board.</param>
    /// <returns>The appropriate corner or edge character.</returns>
    public static char GetTopLeftCorner(bool isAtTop, bool isAtLeft)
    {
        if (isAtTop && isAtLeft)
        {
            return ConsoleTheme.Border.TopLeft;
        }

        if (isAtTop)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        if (isAtLeft)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return ConsoleTheme.ViewportBorder.DiagonalTopLeft;
    }

    /// <summary>
    /// Gets the character for the top-right corner based on viewport edge state.
    /// </summary>
    /// <param name="isAtTop">Whether the viewport is at the top edge of the board.</param>
    /// <param name="isAtRight">Whether the viewport is at the right edge of the board.</param>
    /// <returns>The appropriate corner or edge character.</returns>
    public static char GetTopRightCorner(bool isAtTop, bool isAtRight)
    {
        if (isAtTop && isAtRight)
        {
            return ConsoleTheme.Border.TopRight;
        }

        if (isAtTop)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        if (isAtRight)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return ConsoleTheme.ViewportBorder.DiagonalTopRight;
    }

    /// <summary>
    /// Gets the character for the bottom-left corner based on viewport edge state.
    /// </summary>
    /// <param name="isAtBottom">Whether the viewport is at the bottom edge of the board.</param>
    /// <param name="isAtLeft">Whether the viewport is at the left edge of the board.</param>
    /// <returns>The appropriate corner or edge character.</returns>
    public static char GetBottomLeftCorner(bool isAtBottom, bool isAtLeft)
    {
        if (isAtBottom && isAtLeft)
        {
            return ConsoleTheme.Border.BottomLeft;
        }

        if (isAtBottom)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        if (isAtLeft)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return ConsoleTheme.ViewportBorder.DiagonalBottomLeft;
    }

    /// <summary>
    /// Gets the character for the bottom-right corner based on viewport edge state.
    /// </summary>
    /// <param name="isAtBottom">Whether the viewport is at the bottom edge of the board.</param>
    /// <param name="isAtRight">Whether the viewport is at the right edge of the board.</param>
    /// <returns>The appropriate corner or edge character.</returns>
    public static char GetBottomRightCorner(bool isAtBottom, bool isAtRight)
    {
        if (isAtBottom && isAtRight)
        {
            return ConsoleTheme.Border.BottomRight;
        }

        if (isAtBottom)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        if (isAtRight)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return ConsoleTheme.ViewportBorder.DiagonalBottomRight;
    }

    /// <summary>
    /// Gets the foreground color for a border edge based on viewport edge state.
    /// </summary>
    /// <param name="isAtEdge">Whether the viewport is at this edge of the board.</param>
    /// <returns>Gray if at the edge (solid border), dark gray if not (arrow indicators).</returns>
    public static AnsiSequence GetBorderColor(bool isAtEdge) => isAtEdge ? AnsiSequence.ForegroundGray : AnsiSequence.ForegroundDarkGray;

    /// <summary>
    /// Gets the horizontal border character based on viewport edge state.
    /// </summary>
    /// <param name="isAtEdge">Whether the viewport is at the vertical edge (top or bottom).</param>
    /// <param name="isTop">Whether this is the top border (true) or bottom border (false).</param>
    /// <returns>The horizontal bar if at edge, up/down arrow if not.</returns>
    public static char GetHorizontalChar(bool isAtEdge, bool isTop)
    {
        if (isAtEdge)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        return isTop ? ConsoleTheme.ViewportBorder.Up : ConsoleTheme.ViewportBorder.Down;
    }

    /// <summary>
    /// Gets the vertical border character based on viewport edge state.
    /// </summary>
    /// <param name="isAtEdge">Whether the viewport is at the horizontal edge (left or right).</param>
    /// <param name="isLeft">Whether this is the left border (true) or right border (false).</param>
    /// <returns>The vertical bar if at edge, left/right arrow if not.</returns>
    public static char GetVerticalChar(bool isAtEdge, bool isLeft)
    {
        if (isAtEdge)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return isLeft ? ConsoleTheme.ViewportBorder.Left : ConsoleTheme.ViewportBorder.Right;
    }
}
