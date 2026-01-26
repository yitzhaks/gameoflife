namespace GameOfLife.Rendering.Console;

/// <summary>
/// A token representing either a character or an ANSI sequence.
/// This is a discriminated union implemented as a readonly struct.
/// </summary>
public readonly struct Token : IEquatable<Token>
{
    private Token(char c)
    {
        Character = c;
        Sequence = default;
        IsSequence = false;
    }

    private Token(AnsiSequence sequence)
    {
        Character = default;
        Sequence = sequence;
        IsSequence = true;
    }

    /// <summary>
    /// Gets a value indicating whether this token is a character.
    /// </summary>
    public bool IsCharacter => !IsSequence;

    /// <summary>
    /// Gets a value indicating whether this token is an ANSI sequence.
    /// </summary>
    public bool IsSequence { get; }

    /// <summary>
    /// Gets the character value. Only valid when <see cref="IsCharacter"/> is true.
    /// </summary>
    public char Character { get; }

    /// <summary>
    /// Gets the ANSI sequence value. Only valid when <see cref="IsSequence"/> is true.
    /// </summary>
    public AnsiSequence Sequence { get; }

    /// <summary>
    /// Creates a token representing a character.
    /// </summary>
    /// <param name="c">The character.</param>
    /// <returns>A character token.</returns>
    public static Token Char(char c) => new(c);

    /// <summary>
    /// Creates a token representing an ANSI sequence.
    /// </summary>
    /// <param name="seq">The ANSI sequence.</param>
    /// <returns>An ANSI sequence token.</returns>
    public static Token Ansi(AnsiSequence seq) => new(seq);

    /// <inheritdoc/>
    public bool Equals(Token other)
    {
        return IsSequence == other.IsSequence &&
            (IsSequence ? Sequence == other.Sequence : Character == other.Character);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Token other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => IsSequence ? HashCode.Combine(true, Sequence) : HashCode.Combine(false, Character);

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Token left, Token right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Token left, Token right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => IsSequence ? $"Ansi({Sequence})" : $"Char('{Character}')";
}

/// <summary>
/// Well-known token instances for common values.
/// </summary>
public static class WellKnownTokens
{
    /// <summary>
    /// Newline character token.
    /// </summary>
    public static readonly Token Newline = Token.Char('\n');

    /// <summary>
    /// Space character token.
    /// </summary>
    public static readonly Token Space = Token.Char(' ');

    /// <summary>
    /// Green foreground color token (for alive cells).
    /// </summary>
    public static readonly Token Green = Token.Ansi(AnsiSequence.ForegroundGreen);

    /// <summary>
    /// Dark gray foreground color token (for dead cells).
    /// </summary>
    public static readonly Token DarkGray = Token.Ansi(AnsiSequence.ForegroundDarkGray);

    /// <summary>
    /// Gray foreground color token (for borders).
    /// </summary>
    public static readonly Token Gray = Token.Ansi(AnsiSequence.ForegroundGray);

    /// <summary>
    /// Reset color token.
    /// </summary>
    public static readonly Token Reset = Token.Ansi(AnsiSequence.Reset);
}
