namespace GameOfLife.Rendering.Console;

/// <summary>
/// A zero-allocation enumerator that normalizes glyph colors based on tracked state.
/// </summary>
/// <remarks>
/// This ref struct wraps a GlyphEnumerator and tracks the current color state.
/// It normalizes glyphs so that each glyph has the actual color it should be rendered with,
/// regardless of whether the color was explicitly set for that glyph.
/// Throws on unknown ANSI sequences to ensure we don't silently mishandle color state.
/// </remarks>
public ref struct ColorNormalizedGlyphEnumerator
{
    private GlyphEnumerator _glyphEnumerator;
    private AnsiSequence? _currentColor;

    /// <summary>
    /// Creates a new color-normalized glyph enumerator.
    /// </summary>
    /// <param name="glyphEnumerator">The glyph enumerator to wrap.</param>
    public ColorNormalizedGlyphEnumerator(GlyphEnumerator glyphEnumerator)
    {
        _glyphEnumerator = glyphEnumerator;
        Current = default;
        _currentColor = null;
    }

    /// <summary>
    /// Gets the current normalized glyph.
    /// </summary>
    public Glyph Current { get; private set; }

    /// <summary>
    /// Advances the enumerator to the next glyph.
    /// </summary>
    /// <returns>True if there is a next glyph; false if the enumeration is complete.</returns>
    public bool MoveNext()
    {
        if (!_glyphEnumerator.MoveNext())
        {
            return false;
        }

        var glyph = _glyphEnumerator.Current;

        if (glyph.IsNewline)
        {
            // Newlines don't have color state
            Current = glyph;
            return true;
        }

        // Update color state if the glyph has a color
        if (glyph.Color.HasValue)
        {
            ValidateSequence(glyph.Color.Value);
            _currentColor = glyph.Color.Value;
        }

        // Normalize the glyph with the current color state
        Current = new Glyph(_currentColor, glyph.Character);
        return true;
    }

    /// <summary>
    /// Returns this enumerator (supports foreach).
    /// </summary>
    public readonly ColorNormalizedGlyphEnumerator GetEnumerator() => this;

    private static void ValidateSequence(AnsiSequence sequence)
    {
        // Validate that we know how to handle this sequence
        // This throws for any sequence not in our known set
        switch (sequence)
        {
            case AnsiSequence.Reset:
            case AnsiSequence.ForegroundGreen:
            case AnsiSequence.ForegroundDarkGray:
            case AnsiSequence.ForegroundGray:
                // Known sequences - ok
                break;
            default:
                throw new InvalidOperationException($"Unknown ANSI sequence: {sequence}");
        }
    }
}

/// <summary>
/// Provides methods for tracking ANSI state across glyph streams.
/// </summary>
public static class AnsiStateTracker
{
    /// <summary>
    /// Creates a color-normalized glyph enumerator from a glyph enumerator.
    /// </summary>
    /// <param name="glyphEnumerator">The glyph enumerator to read from.</param>
    /// <returns>A color-normalized glyph enumerator.</returns>
    public static ColorNormalizedGlyphEnumerator FromGlyphs(GlyphEnumerator glyphEnumerator) => new(glyphEnumerator);
}
