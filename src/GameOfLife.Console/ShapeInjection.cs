namespace GameOfLife.Console;

/// <summary>
/// Represents a shape to inject at a specific position.
/// </summary>
/// <param name="PatternName">The name of the pattern file (without extension).</param>
/// <param name="X">The X coordinate to inject at.</param>
/// <param name="Y">The Y coordinate to inject at.</param>
internal readonly record struct ShapeInjection(string PatternName, int X, int Y)
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

        var atIndex = value.IndexOf('@', StringComparison.Ordinal);
        if (atIndex < 0)
        {
            throw new FormatException($"Invalid shape injection format '{value}'. Expected format: name@x,y");
        }

        var name = value[..atIndex];
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new FormatException($"Invalid shape injection format '{value}'. Pattern name cannot be empty.");
        }

        var coordinates = value[(atIndex + 1)..];
        var commaIndex = coordinates.IndexOf(',', StringComparison.Ordinal);
        if (commaIndex < 0)
        {
            throw new FormatException($"Invalid shape injection format '{value}'. Expected format: name@x,y");
        }

        var xPart = coordinates[..commaIndex];
        var yPart = coordinates[(commaIndex + 1)..];

        if (!int.TryParse(xPart, out var x))
        {
            throw new FormatException($"Invalid X coordinate '{xPart}' in shape injection '{value}'.");
        }

        if (!int.TryParse(yPart, out var y))
        {
            throw new FormatException($"Invalid Y coordinate '{yPart}' in shape injection '{value}'.");
        }

        return new ShapeInjection(name, x, y);
    }
}
