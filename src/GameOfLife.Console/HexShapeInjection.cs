using GameOfLife.Core;

namespace GameOfLife.Console;

/// <summary>
/// Represents a hex shape to inject at a specific position.
/// </summary>
/// <param name="PatternName">The name of the pattern file (without extension).</param>
/// <param name="Position">The position to inject at (Q,R coordinates).</param>
internal readonly record struct HexShapeInjection(string PatternName, HexPoint Position)
{
    /// <summary>
    /// Parses a hex shape injection from string format "name@q,r".
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed hex shape injection.</returns>
    /// <exception cref="FormatException">Thrown if the string is not in the expected format.</exception>
    public static HexShapeInjection Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        int atIndex = value.IndexOf('@', StringComparison.Ordinal);
        if (atIndex < 0)
        {
            throw new FormatException($"Invalid hex shape injection format '{value}'. Expected format: name@q,r");
        }

        string name = value[..atIndex];
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new FormatException($"Invalid hex shape injection format '{value}'. Pattern name cannot be empty.");
        }

        string coordinates = value[(atIndex + 1)..];
        int commaIndex = coordinates.IndexOf(',', StringComparison.Ordinal);
        if (commaIndex < 0)
        {
            throw new FormatException($"Invalid hex shape injection format '{value}'. Expected format: name@q,r");
        }

        string qPart = coordinates[..commaIndex];
        string rPart = coordinates[(commaIndex + 1)..];

        if (!int.TryParse(qPart, out int q))
        {
            throw new FormatException($"Invalid Q coordinate '{qPart}' in hex shape injection '{value}'.");
        }

        if (!int.TryParse(rPart, out int r))
        {
            throw new FormatException($"Invalid R coordinate '{rPart}' in hex shape injection '{value}'.");
        }

        return new HexShapeInjection(name, (q, r));
    }
}
