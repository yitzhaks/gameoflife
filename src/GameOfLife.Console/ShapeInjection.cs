using GameOfLife.Core;

namespace GameOfLife.Console;

/// <summary>
/// Represents a shape to inject at a specific position.
/// </summary>
/// <param name="PatternName">The name of the pattern file (without extension).</param>
/// <param name="Position">The position to inject at.</param>
internal readonly record struct ShapeInjection(string PatternName, Point2D Position)
{
    /// <summary>
    /// Parses a shape injection from string format "name@x,y".
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed shape injection.</returns>
    /// <exception cref="FormatException">Thrown if the string is not in the expected format.</exception>
    public static ShapeInjection Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        int atIndex = value.IndexOf('@', StringComparison.Ordinal);
        if (atIndex < 0)
        {
            throw new FormatException($"Invalid shape injection format '{value}'. Expected format: name@x,y");
        }

        string name = value[..atIndex];
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new FormatException($"Invalid shape injection format '{value}'. Pattern name cannot be empty.");
        }

        string coordinates = value[(atIndex + 1)..];
        int commaIndex = coordinates.IndexOf(',', StringComparison.Ordinal);
        if (commaIndex < 0)
        {
            throw new FormatException($"Invalid shape injection format '{value}'. Expected format: name@x,y");
        }

        string xPart = coordinates[..commaIndex];
        string yPart = coordinates[(commaIndex + 1)..];

        if (!int.TryParse(xPart, out int x))
        {
            throw new FormatException($"Invalid X coordinate '{xPart}' in shape injection '{value}'.");
        }

        if (!int.TryParse(yPart, out int y))
        {
            throw new FormatException($"Invalid Y coordinate '{yPart}' in shape injection '{value}'.");
        }

        return new ShapeInjection(name, (x, y));
    }
}
