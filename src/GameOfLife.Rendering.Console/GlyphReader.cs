namespace GameOfLife.Rendering.Console;

/// <summary>
/// A zero-allocation enumerator that groups tokens into glyphs.
/// </summary>
/// <remarks>
/// This ref struct wraps a TokenEnumerator and collects ANSI sequences
/// until a character is found, then yields a Glyph with the accumulated color.
/// </remarks>
public ref struct GlyphEnumerator
{
    private TokenEnumerator _tokenEnumerator;
    private AnsiSequence? _pendingColor;
    private AnsiSequence? _pendingBackgroundColor;

    /// <summary>
    /// Creates a new glyph enumerator wrapping the specified token enumerator.
    /// </summary>
    /// <param name="tokenEnumerator">The token enumerator to wrap.</param>
    public GlyphEnumerator(TokenEnumerator tokenEnumerator)
    {
        _tokenEnumerator = tokenEnumerator;
        Current = default;
        _pendingColor = null;
        _pendingBackgroundColor = null;
    }

    /// <summary>
    /// Gets the current glyph.
    /// </summary>
    public Glyph Current { get; private set; }

    /// <summary>
    /// Advances the enumerator to the next glyph.
    /// </summary>
    /// <returns>True if there is a next glyph; false if the enumeration is complete.</returns>
    public bool MoveNext()
    {
        while (_tokenEnumerator.MoveNext())
        {
            Token token = _tokenEnumerator.Current;

            if (token.IsSequence)
            {
                // Accumulate color sequences
                AnsiSequence seq = token.Sequence;
                if (seq == AnsiSequence.Reset)
                {
                    _pendingColor = null;
                    _pendingBackgroundColor = null;
                }
                else if (seq is AnsiSequence.BackgroundGreen or AnsiSequence.BackgroundDarkGray)
                {
                    _pendingBackgroundColor = seq;
                }
                else if (seq == AnsiSequence.BackgroundDefault)
                {
                    // Preserve BackgroundDefault so downstream can emit reset sequence
                    _pendingBackgroundColor = AnsiSequence.BackgroundDefault;
                }
                else
                {
                    _pendingColor = seq;
                }
            }
            else
            {
                // Found a character - yield a glyph
                char character = token.Character;

                if (character == '\n')
                {
                    // Newlines have no color
                    Current = new Glyph(null, null, '\n');
                }
                else
                {
                    Current = new Glyph(_pendingColor, _pendingBackgroundColor, character);
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns this enumerator (supports foreach).
    /// </summary>
    public readonly GlyphEnumerator GetEnumerator() => this;
}

/// <summary>
/// A zero-allocation enumerator that groups half-block tokens into glyphs.
/// </summary>
/// <remarks>
/// This ref struct wraps a HalfBlockTokenEnumerator and collects ANSI sequences
/// until a character is found, then yields a Glyph with the accumulated color.
/// </remarks>
public ref struct HalfBlockGlyphEnumerator
{
    private HalfBlockTokenEnumerator _tokenEnumerator;
    private AnsiSequence? _pendingColor;
    private AnsiSequence? _pendingBackgroundColor;

    /// <summary>
    /// Creates a new glyph enumerator wrapping the specified half-block token enumerator.
    /// </summary>
    /// <param name="tokenEnumerator">The half-block token enumerator to wrap.</param>
    public HalfBlockGlyphEnumerator(HalfBlockTokenEnumerator tokenEnumerator)
    {
        _tokenEnumerator = tokenEnumerator;
        Current = default;
        _pendingColor = null;
        _pendingBackgroundColor = null;
    }

    /// <summary>
    /// Gets the current glyph.
    /// </summary>
    public Glyph Current { get; private set; }

    /// <summary>
    /// Advances the enumerator to the next glyph.
    /// </summary>
    /// <returns>True if there is a next glyph; false if the enumeration is complete.</returns>
    public bool MoveNext()
    {
        while (_tokenEnumerator.MoveNext())
        {
            Token token = _tokenEnumerator.Current;

            if (token.IsSequence)
            {
                // Accumulate color sequences
                AnsiSequence seq = token.Sequence;
                if (seq == AnsiSequence.Reset)
                {
                    _pendingColor = null;
                    _pendingBackgroundColor = null;
                }
                else if (seq is AnsiSequence.BackgroundGreen or AnsiSequence.BackgroundDarkGray or AnsiSequence.BackgroundDefault)
                {
                    // Preserve BackgroundDefault so downstream can emit reset sequence
                    _pendingBackgroundColor = seq;
                }
                else
                {
                    _pendingColor = seq;
                }
            }
            else
            {
                // Found a character - yield a glyph
                char character = token.Character;

                if (character == '\n')
                {
                    // Newlines have no color
                    Current = new Glyph(null, null, '\n');
                }
                else
                {
                    Current = new Glyph(_pendingColor, _pendingBackgroundColor, character);
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns this enumerator (supports foreach).
    /// </summary>
    public readonly HalfBlockGlyphEnumerator GetEnumerator() => this;
}

/// <summary>
/// Provides methods for reading glyphs from token streams.
/// </summary>
public static class GlyphReader
{
    /// <summary>
    /// Creates a glyph enumerator from a token enumerator.
    /// </summary>
    /// <param name="tokenEnumerator">The token enumerator to read from.</param>
    /// <returns>A glyph enumerator.</returns>
    public static GlyphEnumerator FromTokens(TokenEnumerator tokenEnumerator) => new(tokenEnumerator);

    /// <summary>
    /// Creates a glyph enumerator from a half-block token enumerator.
    /// </summary>
    /// <param name="tokenEnumerator">The half-block token enumerator to read from.</param>
    /// <returns>A half-block glyph enumerator.</returns>
    public static HalfBlockGlyphEnumerator FromTokensHalfBlock(HalfBlockTokenEnumerator tokenEnumerator) => new(tokenEnumerator);
}
