namespace GameOfLife.Rendering.Console;

/// <summary>
/// Provides streaming differential rendering between two glyph streams.
/// </summary>
public static class StreamingDiff
{
    /// <summary>
    /// Applies differences between two glyph streams to the output.
    /// Only writes changes where glyphs differ.
    /// </summary>
    /// <param name="previous">The previous frame's glyph enumerator.</param>
    /// <param name="current">The current frame's glyph enumerator.</param>
    /// <param name="output">The text writer to output changes to.</param>
    /// <param name="startRow">The starting row for the board (1-indexed). Default is 1.</param>
    /// <remarks>
    /// Uses ANSI escape sequences for cursor positioning.
    /// Position tracking uses 1-indexed row/column (ANSI standard).
    /// Newlines increment row and reset column.
    /// Supports drawing a larger board over a smaller one, but not vice versa.
    /// </remarks>
    public static void Apply(
        ref ColorNormalizedGlyphEnumerator previous,
        ref ColorNormalizedGlyphEnumerator current,
        TextWriter output,
        int startRow = 1)
    {
        ArgumentNullException.ThrowIfNull(output);

        int row = startRow;
        int col = 1;
        AnsiSequence? lastWrittenColor = null;

        // Track where the cursor actually is to avoid redundant positioning
        int cursorRow = -1;
        int cursorCol = -1;

        while (current.MoveNext())
        {
            bool hasPrev = previous.MoveNext();
            Glyph prevGlyph = hasPrev ? previous.Current : default;
            Glyph currGlyph = current.Current;

            if (currGlyph.IsNewline)
            {
                row++;
                col = 1;
            }
            else if (!hasPrev || !currGlyph.Equals(prevGlyph))
            {
                // Glyph changed - only emit cursor position if not already there
                if (cursorRow != row || cursorCol != col)
                {
                    output.Write($"\x1b[{row};{col}H");
                    cursorRow = row;
                    cursorCol = col;
                }

                // Emit color if different from last written
                if (currGlyph.Color.HasValue && currGlyph.Color != lastWrittenColor)
                {
                    output.Write(currGlyph.Color.Value.ToAnsiString());
                    lastWrittenColor = currGlyph.Color;
                }

                output.Write(currGlyph.Character);
                cursorCol++; // Cursor advances after write
                col++;
            }
            else
            {
                // Same glyph, just advance position
                col++;
            }
        }
    }

    /// <summary>
    /// Writes the full frame to output (no diffing, for first frame).
    /// </summary>
    /// <param name="current">The glyph enumerator for the frame.</param>
    /// <param name="output">The text writer to output to.</param>
    /// <param name="startRow">The starting row for the board (1-indexed). Default is 1.</param>
    public static void WriteFull(
        ref ColorNormalizedGlyphEnumerator current,
        TextWriter output,
        int startRow = 1)
    {
        ArgumentNullException.ThrowIfNull(output);

        int row = startRow;
        int col = 1;
        AnsiSequence? lastWrittenColor = null;

        // Position cursor at start
        output.Write($"\x1b[{row};{col}H");

        while (current.MoveNext())
        {
            Glyph glyph = current.Current;

            if (glyph.IsNewline)
            {
                // Write actual newline (will be translated by caller if needed)
                output.Write('\n');
                row++;
                col = 1;
            }
            else
            {
                // Emit color if different from last written
                if (glyph.Color.HasValue && glyph.Color != lastWrittenColor)
                {
                    output.Write(glyph.Color.Value.ToAnsiString());
                    lastWrittenColor = glyph.Color;
                }

                output.Write(glyph.Character);
                col++;
            }
        }
    }

    /// <summary>
    /// Writes the full frame to output and captures glyphs to a buffer for future diffing.
    /// </summary>
    /// <param name="current">The glyph enumerator for the frame.</param>
    /// <param name="output">The text writer to output to.</param>
    /// <param name="frameBuffer">The list to store glyphs in. Will be cleared first.</param>
    /// <param name="startRow">The starting row for the board (1-indexed). Default is 1.</param>
    public static void WriteFullAndCapture(
        ref ColorNormalizedGlyphEnumerator current,
        TextWriter output,
        IList<Glyph> frameBuffer,
        int startRow = 1)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(frameBuffer);

        frameBuffer.Clear();

        int row = startRow;
        int col = 1;
        AnsiSequence? lastWrittenColor = null;

        // Position cursor at start
        output.Write($"\x1b[{row};{col}H");

        while (current.MoveNext())
        {
            Glyph glyph = current.Current;
            frameBuffer.Add(new Glyph(glyph.Color, glyph.Character));

            if (glyph.IsNewline)
            {
                output.Write('\n');
                row++;
                col = 1;
            }
            else
            {
                if (glyph.Color.HasValue && glyph.Color != lastWrittenColor)
                {
                    output.Write(glyph.Color.Value.ToAnsiString());
                    lastWrittenColor = glyph.Color;
                }

                output.Write(glyph.Character);
                col++;
            }
        }
    }

    /// <summary>
    /// Applies differences between a cached frame buffer and a glyph stream.
    /// </summary>
    /// <param name="previousFrame">The cached previous frame glyphs.</param>
    /// <param name="current">The current frame's glyph enumerator.</param>
    /// <param name="output">The text writer to output changes to.</param>
    /// <param name="frameBuffer">The list to store current frame glyphs in. Will be cleared first.</param>
    /// <param name="startRow">The starting row for the board (1-indexed). Default is 1.</param>
    public static void ApplyAndCapture(
        IReadOnlyList<Glyph> previousFrame,
        ref ColorNormalizedGlyphEnumerator current,
        TextWriter output,
        IList<Glyph> frameBuffer,
        int startRow = 1)
    {
        ArgumentNullException.ThrowIfNull(previousFrame);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(frameBuffer);

        frameBuffer.Clear();

        int row = startRow;
        int col = 1;
        int prevIndex = 0;
        AnsiSequence? lastWrittenColor = null;

        // Track where the cursor actually is to avoid redundant positioning
        int cursorRow = -1;
        int cursorCol = -1;

        while (current.MoveNext())
        {
            Glyph currGlyph = current.Current;
            frameBuffer.Add(new Glyph(currGlyph.Color, currGlyph.Character));

            bool hasPrev = prevIndex < previousFrame.Count;
            Glyph prevGlyph = hasPrev ? previousFrame[prevIndex] : default;
            prevIndex++;

            if (currGlyph.IsNewline)
            {
                row++;
                col = 1;
            }
            else if (!hasPrev || currGlyph.Color != prevGlyph.Color || currGlyph.Character != prevGlyph.Character)
            {
                // Glyph changed - only emit cursor position if not already there
                if (cursorRow != row || cursorCol != col)
                {
                    output.Write($"\x1b[{row};{col}H");
                    cursorRow = row;
                    cursorCol = col;
                }

                if (currGlyph.Color.HasValue && currGlyph.Color != lastWrittenColor)
                {
                    output.Write(currGlyph.Color.Value.ToAnsiString());
                    lastWrittenColor = currGlyph.Color;
                }

                output.Write(currGlyph.Character);
                cursorCol++; // Cursor advances after write
                col++;
            }
            else
            {
                col++;
            }
        }
    }
}
