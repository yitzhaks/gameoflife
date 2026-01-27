namespace GameOfLife.Rendering.Console;

/// <summary>
/// Known ANSI color sequences used for console rendering.
/// </summary>
public enum AnsiSequence : byte
{
    /// <summary>
    /// Reset all attributes to default (\x1b[0m).
    /// </summary>
    Reset,

    /// <summary>
    /// Green foreground color for alive cells (\x1b[32m).
    /// </summary>
    ForegroundGreen,

    /// <summary>
    /// Dark gray foreground color for dead cells (\x1b[90m).
    /// </summary>
    ForegroundDarkGray,

    /// <summary>
    /// Gray foreground color for borders (\x1b[37m).
    /// </summary>
    ForegroundGray,
}

/// <summary>
/// Extension methods for <see cref="AnsiSequence"/>.
/// </summary>
public static class AnsiSequenceExtensions
{
    /// <summary>
    /// Converts an ANSI sequence to its string representation.
    /// </summary>
    /// <param name="seq">The ANSI sequence to convert.</param>
    /// <returns>The ANSI escape string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for unknown sequences.</exception>
    public static string ToAnsiString(this AnsiSequence seq) => seq switch
    {
        AnsiSequence.Reset => "\x1b[0m",
        AnsiSequence.ForegroundGreen => "\x1b[32m",
        AnsiSequence.ForegroundDarkGray => "\x1b[90m",
        AnsiSequence.ForegroundGray => "\x1b[37m",
        _ => throw new ArgumentOutOfRangeException(nameof(seq), seq, "Unknown ANSI sequence")
    };
}
