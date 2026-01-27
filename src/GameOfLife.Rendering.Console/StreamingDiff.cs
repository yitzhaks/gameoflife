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
        AnsiSequence? lastWrittenBackground = null;

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

                // Emit foreground color if different from last written
                if (currGlyph.Color.HasValue && currGlyph.Color != lastWrittenColor)
                {
                    output.Write(currGlyph.Color.Value.ToAnsiString());
                    lastWrittenColor = currGlyph.Color;
                }

                // Emit background color if different from last written
                if (currGlyph.BackgroundColor.HasValue && currGlyph.BackgroundColor != lastWrittenBackground)
                {
                    output.Write(currGlyph.BackgroundColor.Value.ToAnsiString());
                    lastWrittenBackground = currGlyph.BackgroundColor;
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
        AnsiSequence? lastWrittenBackground = null;

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
                // Emit foreground color if different from last written
                if (glyph.Color.HasValue && glyph.Color != lastWrittenColor)
                {
                    output.Write(glyph.Color.Value.ToAnsiString());
                    lastWrittenColor = glyph.Color;
                }

                // Emit background color if different from last written
                if (glyph.BackgroundColor.HasValue && glyph.BackgroundColor != lastWrittenBackground)
                {
                    output.Write(glyph.BackgroundColor.Value.ToAnsiString());
                    lastWrittenBackground = glyph.BackgroundColor;
                }

                output.Write(glyph.Character);
                col++;
            }
        }
    }

    /// <summary>
    /// Writes the full frame to output and captures glyphs to a pre-allocated buffer.
    /// </summary>
    /// <param name="current">The glyph enumerator for the frame.</param>
    /// <param name="output">The text writer to output to.</param>
    /// <param name="frameBuffer">The pre-allocated buffer to store glyphs in. Will be cleared first.</param>
    /// <param name="startRow">The starting row for the board (1-indexed). Default is 1.</param>
    public static void WriteFullAndCapture(
        ref ColorNormalizedGlyphEnumerator current,
        TextWriter output,
        FrameBuffer frameBuffer,
        int startRow = 1)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(frameBuffer);

        ApplyAndCaptureCore(previousFrame: null, ref current, output, frameBuffer, startRow);
    }

    /// <summary>
    /// Applies differences between a pre-allocated frame buffer and a glyph stream.
    /// Zero-allocation diffing using pre-allocated buffers.
    /// </summary>
    /// <param name="previousFrame">The cached previous frame buffer.</param>
    /// <param name="current">The current frame's glyph enumerator.</param>
    /// <param name="output">The text writer to output changes to.</param>
    /// <param name="currentFrame">The pre-allocated buffer to store current frame. Will be cleared first.</param>
    /// <param name="startRow">The starting row for the board (1-indexed). Default is 1.</param>
    public static void ApplyAndCapture(
        FrameBuffer previousFrame,
        ref ColorNormalizedGlyphEnumerator current,
        TextWriter output,
        FrameBuffer currentFrame,
        int startRow = 1)
    {
        ArgumentNullException.ThrowIfNull(previousFrame);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(currentFrame);

        ApplyAndCaptureCore(previousFrame, ref current, output, currentFrame, startRow);
    }

    private static void ApplyAndCaptureCore(
        FrameBuffer? previousFrame,
        ref ColorNormalizedGlyphEnumerator current,
        TextWriter output,
        FrameBuffer currentFrame,
        int startRow)
    {
        currentFrame.Clear();

        bool isFullWrite = previousFrame is null || previousFrame.Count == 0;
        int row = startRow;
        int col = 1;
        int prevIndex = 0;
        AnsiSequence? lastWrittenColor = null;
        AnsiSequence? lastWrittenBackground = null;

        // Track where the cursor actually is to avoid redundant positioning
        int cursorRow = -1;
        int cursorCol = -1;

        // For full write, position cursor at start
        if (isFullWrite)
        {
            output.Write($"\x1b[{row};{col}H");
            cursorRow = row;
            cursorCol = col;
        }

        while (current.MoveNext())
        {
            Glyph currGlyph = current.Current;
            currentFrame.Add(currGlyph);

            if (currGlyph.IsNewline)
            {
                if (isFullWrite)
                {
                    output.Write('\n');
                }

                prevIndex++; // Newlines are stored in buffer, so must increment
                row++;
                col = 1;
                if (isFullWrite)
                {
                    cursorRow = row;
                    cursorCol = col;
                }
            }
            else
            {
                bool needsWrite = isFullWrite;
                if (!isFullWrite)
                {
                    bool hasPrev = prevIndex < previousFrame!.Count;
                    Glyph prevGlyph = hasPrev ? previousFrame[prevIndex] : default;
                    needsWrite = !hasPrev || currGlyph.Color != prevGlyph.Color || currGlyph.BackgroundColor != prevGlyph.BackgroundColor || currGlyph.Character != prevGlyph.Character;
                }

                prevIndex++;

                if (needsWrite)
                {
                    // Only emit cursor position if not already there
                    if (cursorRow != row || cursorCol != col)
                    {
                        output.Write($"\x1b[{row};{col}H");
                        cursorRow = row;
                        cursorCol = col;
                    }

                    // Emit foreground color if different from last written
                    if (currGlyph.Color.HasValue && currGlyph.Color != lastWrittenColor)
                    {
                        output.Write(currGlyph.Color.Value.ToAnsiString());
                        lastWrittenColor = currGlyph.Color;
                    }

                    // Emit background color if different from last written
                    if (currGlyph.BackgroundColor.HasValue && currGlyph.BackgroundColor != lastWrittenBackground)
                    {
                        output.Write(currGlyph.BackgroundColor.Value.ToAnsiString());
                        lastWrittenBackground = currGlyph.BackgroundColor;
                    }

                    output.Write(currGlyph.Character);
                    cursorCol++;
                }

                col++;
            }
        }
    }

    /// <summary>
    /// Writes the full frame to output and captures glyphs to a pre-allocated buffer (half-block version).
    /// </summary>
    /// <param name="current">The half-block glyph enumerator for the frame.</param>
    /// <param name="output">The text writer to output to.</param>
    /// <param name="frameBuffer">The pre-allocated buffer to store glyphs in. Will be cleared first.</param>
    /// <param name="startRow">The starting row for the board (1-indexed). Default is 1.</param>
    public static void WriteFullAndCaptureHalfBlock(
        ref HalfBlockColorNormalizedGlyphEnumerator current,
        TextWriter output,
        FrameBuffer frameBuffer,
        int startRow = 1)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(frameBuffer);

        ApplyAndCaptureCoreHalfBlock(previousFrame: null, ref current, output, frameBuffer, startRow);
    }

    /// <summary>
    /// Applies differences between a pre-allocated frame buffer and a half-block glyph stream.
    /// Zero-allocation diffing using pre-allocated buffers.
    /// </summary>
    /// <param name="previousFrame">The cached previous frame buffer.</param>
    /// <param name="current">The current frame's half-block glyph enumerator.</param>
    /// <param name="output">The text writer to output changes to.</param>
    /// <param name="currentFrame">The pre-allocated buffer to store current frame. Will be cleared first.</param>
    /// <param name="startRow">The starting row for the board (1-indexed). Default is 1.</param>
    public static void ApplyAndCaptureHalfBlock(
        FrameBuffer previousFrame,
        ref HalfBlockColorNormalizedGlyphEnumerator current,
        TextWriter output,
        FrameBuffer currentFrame,
        int startRow = 1)
    {
        ArgumentNullException.ThrowIfNull(previousFrame);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(currentFrame);

        ApplyAndCaptureCoreHalfBlock(previousFrame, ref current, output, currentFrame, startRow);
    }

    private static void ApplyAndCaptureCoreHalfBlock(
        FrameBuffer? previousFrame,
        ref HalfBlockColorNormalizedGlyphEnumerator current,
        TextWriter output,
        FrameBuffer currentFrame,
        int startRow)
    {
        currentFrame.Clear();

        bool isFullWrite = previousFrame is null || previousFrame.Count == 0;
        int row = startRow;
        int col = 1;
        int prevIndex = 0;
        AnsiSequence? lastWrittenColor = null;
        AnsiSequence? lastWrittenBackground = null;

        // Track where the cursor actually is to avoid redundant positioning
        int cursorRow = -1;
        int cursorCol = -1;

        // For full write, position cursor at start
        if (isFullWrite)
        {
            output.Write($"\x1b[{row};{col}H");
            cursorRow = row;
            cursorCol = col;
        }

        while (current.MoveNext())
        {
            Glyph currGlyph = current.Current;
            currentFrame.Add(currGlyph);

            if (currGlyph.IsNewline)
            {
                if (isFullWrite)
                {
                    output.Write('\n');
                }

                prevIndex++; // Newlines are stored in buffer, so must increment
                row++;
                col = 1;
                if (isFullWrite)
                {
                    cursorRow = row;
                    cursorCol = col;
                }
            }
            else
            {
                bool needsWrite = isFullWrite;
                if (!isFullWrite)
                {
                    bool hasPrev = prevIndex < previousFrame!.Count;
                    Glyph prevGlyph = hasPrev ? previousFrame[prevIndex] : default;
                    needsWrite = !hasPrev || currGlyph.Color != prevGlyph.Color || currGlyph.BackgroundColor != prevGlyph.BackgroundColor || currGlyph.Character != prevGlyph.Character;
                }

                prevIndex++;

                if (needsWrite)
                {
                    // Only emit cursor position if not already there
                    if (cursorRow != row || cursorCol != col)
                    {
                        output.Write($"\x1b[{row};{col}H");
                        cursorRow = row;
                        cursorCol = col;
                    }

                    // Emit foreground color if different from last written
                    if (currGlyph.Color.HasValue && currGlyph.Color != lastWrittenColor)
                    {
                        output.Write(currGlyph.Color.Value.ToAnsiString());
                        lastWrittenColor = currGlyph.Color;
                    }

                    // Emit background color if different from last written
                    if (currGlyph.BackgroundColor.HasValue && currGlyph.BackgroundColor != lastWrittenBackground)
                    {
                        output.Write(currGlyph.BackgroundColor.Value.ToAnsiString());
                        lastWrittenBackground = currGlyph.BackgroundColor;
                    }

                    output.Write(currGlyph.Character);
                    cursorCol++;
                }

                col++;
            }
        }
    }
}
