namespace GameOfLife.Rendering.Console;

/// <summary>
/// A displayable unit consisting of a color and a character.
/// </summary>
public readonly struct Glyph : IEquatable<Glyph>
{
    /// <summary>
    /// Creates a new glyph with the specified color and character.
    /// </summary>
    /// <param name="color">The color of the glyph, or null for no color.</param>
    /// <param name="character">The character to display.</param>
    public Glyph(AnsiSequence? color, char character)
    {
        Color = color;
        Character = character;
    }

    /// <summary>
    /// Gets the color of this glyph. Null for newlines or when no color is set.
    /// </summary>
    public AnsiSequence? Color { get; }

    /// <summary>
    /// Gets the character to display.
    /// </summary>
    public char Character { get; }

    /// <summary>
    /// Gets a value indicating whether this glyph represents a newline.
    /// </summary>
    public bool IsNewline => Character == '\n';

    /// <inheritdoc/>
    public bool Equals(Glyph other) => Color == other.Color && Character == other.Character;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Glyph other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Color, Character);

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Glyph left, Glyph right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Glyph left, Glyph right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString()
    {
        return IsNewline ? "Glyph(\\n)" : $"Glyph({Color?.ToString() ?? "null"}, '{Character}')";
    }
}
