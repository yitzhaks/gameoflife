namespace GameOfLife.Rendering.Console;

/// <summary>
/// Configuration for console rendering appearance.
/// </summary>
/// <param name="AliveChar">The character to display for alive cells.</param>
/// <param name="DeadChar">The character to display for dead cells.</param>
/// <param name="AliveColor">The console color for alive cells.</param>
/// <param name="DeadColor">The console color for dead cells.</param>
/// <param name="BorderColor">The console color for the border.</param>
/// <param name="ViewportBorderColor">The console color for viewport borders (arrow indicators).</param>
/// <param name="ShowBorder">Whether to display a border around the grid.</param>
public sealed record ConsoleTheme(
    char AliveChar = '█',
    char DeadChar = '·',
    ConsoleColor AliveColor = ConsoleColor.Green,
    ConsoleColor DeadColor = ConsoleColor.DarkGray,
    ConsoleColor BorderColor = ConsoleColor.Gray,
    ConsoleColor ViewportBorderColor = ConsoleColor.DarkGray,
    bool ShowBorder = true)
{
    /// <summary>
    /// Gets the default console theme.
    /// </summary>
    public static ConsoleTheme Default { get; } = new();

    /// <summary>
    /// Box-drawing characters for the border.
    /// </summary>
    internal static class Border
    {
        public const char TopLeft = '╔';
        public const char TopRight = '╗';
        public const char BottomLeft = '╚';
        public const char BottomRight = '╝';
        public const char Horizontal = '═';
        public const char Vertical = '║';
    }

    /// <summary>
    /// Arrow characters for viewport borders indicating more content in each direction.
    /// </summary>
    internal static class ViewportBorder
    {
        public const char Up = '↑';
        public const char Down = '↓';
        public const char Left = '←';
        public const char Right = '→';
        public const char DiagonalTopLeft = '↖';
        public const char DiagonalTopRight = '↗';
        public const char DiagonalBottomLeft = '↙';
        public const char DiagonalBottomRight = '↘';
    }
}
