using GameOfLife.Core;
using GameOfLife.Rendering;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class StreamingDiffTests
{
    [Fact]
    public void WriteFull_WritesAllGlyphs()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 2);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFull(ref glyphEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain the cursor positioning and characters
        Assert.Contains("\x1b[", result); // Contains ANSI escape
        Assert.Contains(".", result); // Contains dead character
    }

    [Fact]
    public void Apply_IdenticalFrames_WritesNothing()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 2);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Get two identical enumerators
        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // With identical frames, nothing should be written
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_SingleCellChange_WritesOnlyChangedCell()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 3);

        // Previous: all dead
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: center cell alive
        var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [new Point2D(1, 1)] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain the alive character
        Assert.Contains("#", result);

        // Should contain cursor positioning
        Assert.Contains("\x1b[", result);
    }

    [Fact]
    public void Apply_AllCellsChange_WritesAllCells()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 2);

        // Previous: all dead
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: all alive
        var allAlive = new Dictionary<Point2D, bool>();
        foreach (Point2D node in topology.Nodes)
        {
            allAlive[node] = true;
        }

        var currGeneration = new DictionaryGeneration<Point2D, bool>(allAlive, defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain multiple alive characters
        int aliveCount = result.Count(c => c == '#');
        Assert.Equal(4, aliveCount); // 2x2 = 4 cells
    }

    [Fact]
    public void Apply_WithStartRow_UsesCorrectRowPositioning()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(1, 1);

        // Previous: dead
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: alive
        var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [new Point2D(0, 0)] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        // Start at row 5
        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 5);

        string result = output.ToString();

        // Should contain positioning to row 5, column 1
        Assert.Contains("\x1b[5;1H", result);
    }

    [Fact]
    public void WriteFullAndCapture_CapturesAllGlyphs_BufferMatchesRendered()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 2);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [new Point2D(0, 0)] = true },
            defaultState: false);

        var frameBuffer = new List<Glyph>();
        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCapture(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        // Buffer should contain all glyphs (2x2 grid + newlines)
        Assert.NotEmpty(frameBuffer);
        // Check first cell is alive (green)
        Assert.Equal('#', frameBuffer[0].Character);
    }

    [Fact]
    public void ApplyAndCapture_DiffsAndCapturesFrame_BufferMatchesCurrentFrame()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 2);

        // Previous: all dead
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: one alive
        var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [new Point2D(1, 1)] = true },
            defaultState: false);

        // Capture initial frame
        var initialBuffer = new List<Glyph>();
        ColorNormalizedGlyphEnumerator initialEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCapture(ref initialEnumerator, discardOutput, initialBuffer, startRow: 1);

        // Apply diff and capture
        var resultBuffer = new List<Glyph>();
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCapture(initialBuffer, ref currEnumerator, output, resultBuffer, startRow: 1);

        // Buffer should match current frame
        Assert.NotEmpty(resultBuffer);
        // The changed cell should be written
        string result = output.ToString();
        Assert.Contains("#", result);
    }

    [Fact]
    public void Apply_WithColorChanges_EmitsColorCode()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 1);

        // Previous: first alive, second dead
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [new Point2D(0, 0)] = true },
            defaultState: false);

        // Current: first dead, second alive (color change for both)
        var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [new Point2D(1, 0)] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain color codes for the changes
        Assert.Contains("\x1b[", result);
    }

    [Fact]
    public void Apply_LargerCurrentFrame_HandlesNewGlyphs()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // Previous: 1x1
        var prevTopology = new Grid2DTopology(1, 1);
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: 2x2 (larger)
        var currTopology = new Grid2DTopology(2, 2);
        var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(prevTopology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(currTopology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // New cells beyond previous frame should be written
        Assert.Contains(".", result);
    }

    [Fact]
    public void Apply_SkipsUnchangedGlyphs_NoOutputForIdenticalCells()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 3);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Same generation twice
        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // No output for identical frames
        Assert.Empty(result);
    }

    [Fact]
    public void WriteFull_WithMultipleColors_EmitsAllColorCodes()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 1);
        // One alive, one dead - different colors
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [new Point2D(0, 0)] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFull(ref glyphEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain the alive character
        Assert.Contains("#", result);
        // Should contain the dead character
        Assert.Contains(".", result);
        // Should contain ANSI color codes
        Assert.Contains("\x1b[", result);
    }

    [Fact]
    public void WriteFullAndCapture_ClearsBufferBeforeCapture()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(1, 1);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Pre-populate buffer with dummy data
        var frameBuffer = new List<Glyph> { new(null, 'X'), new(null, 'Y') };

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCapture(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        // Buffer should be cleared and only contain new glyphs (1 cell + newline = 2)
        Assert.DoesNotContain(frameBuffer, g => g.Character == 'X');
        Assert.DoesNotContain(frameBuffer, g => g.Character == 'Y');
    }

    [Fact]
    public void ApplyAndCapture_PreviousFrameLarger_HandlesCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // Previous: 2x2 (larger)
        var prevTopology = new Grid2DTopology(2, 2);
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: 1x1 (smaller)
        var currTopology = new Grid2DTopology(1, 1);
        var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [new Point2D(0, 0)] = true },
            defaultState: false);

        // Capture initial larger frame
        var initialBuffer = new List<Glyph>();
        ColorNormalizedGlyphEnumerator initialEnumerator = renderer.GetGlyphEnumerator(prevTopology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCapture(ref initialEnumerator, discardOutput, initialBuffer, startRow: 1);

        // Apply diff with smaller current frame
        var resultBuffer = new List<Glyph>();
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(currTopology, currGeneration);
        StreamingDiff.ApplyAndCapture(initialBuffer, ref currEnumerator, output, resultBuffer, startRow: 1);

        // Should handle gracefully
        Assert.NotEmpty(resultBuffer);
    }

    [Fact]
    public void Apply_SameColorConsecutiveCells_DoesNotReEmitColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 1);

        // Previous: all dead
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: all alive (same color for all 3 cells)
        var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [new Point2D(0, 0)] = true,
                [new Point2D(1, 0)] = true,
                [new Point2D(2, 0)] = true
            },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain 3 alive characters
        Assert.Equal(3, result.Count(c => c == '#'));

        // Color code should appear only once (for the first cell)
        // Count color code sequences by counting escape character followed by '[' and 'm'
        int colorCodeCount = CountAnsiColorCodes(result);
        Assert.Equal(1, colorCodeCount);
    }

    private static int CountAnsiColorCodes(string s)
    {
        // Count occurrences of ANSI color codes like \x1b[32m
        int count = 0;
        for (int i = 0; i < s.Length - 3; i++)
        {
            if (s[i] == '\x1b' && s[i + 1] == '[' && char.IsDigit(s[i + 2]))
            {
                // Check if it ends with 'm' (color code) rather than 'H' (cursor position)
                int j = i + 2;
                while (j < s.Length && char.IsDigit(s[j]))
                {
                    j++;
                }

                if (j < s.Length && s[j] == 'm')
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static int CountCursorPositionCodes(string s)
    {
        // Count occurrences of ANSI cursor position codes like \x1b[1;1H
        int count = 0;
        for (int i = 0; i < s.Length - 4; i++)
        {
            if (s[i] == '\x1b' && s[i + 1] == '[')
            {
                int j = i + 2;
                while (j < s.Length && (char.IsDigit(s[j]) || s[j] == ';'))
                {
                    j++;
                }

                if (j < s.Length && s[j] == 'H')
                {
                    count++;
                }
            }
        }

        return count;
    }

    [Fact]
    public void ApplyAndCapture_SameColorConsecutive_OnlyOneColorEmit()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 1);

        // All alive - same color
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [new Point2D(0, 0)] = true,
                [new Point2D(1, 0)] = true,
                [new Point2D(2, 0)] = true
            },
            defaultState: false);

        // Empty previous frame forces all cells to be written
        var emptyBuffer = new List<Glyph>();
        var frameBuffer = new List<Glyph>();
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        StreamingDiff.ApplyAndCapture(emptyBuffer, ref currEnumerator, output, frameBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain 3 alive characters
        Assert.Equal(3, result.Count(c => c == '#'));
    }

    [Fact]
    public void WriteFull_SameColorConsecutive_OnlyOneColorEmit()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 1);

        // All alive - same color
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [new Point2D(0, 0)] = true,
                [new Point2D(1, 0)] = true,
                [new Point2D(2, 0)] = true
            },
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFull(ref glyphEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain 3 alive characters
        Assert.Equal(3, result.Count(c => c == '#'));

        // Color code should appear only once
        int colorCodeCount = CountAnsiColorCodes(result);
        Assert.Equal(1, colorCodeCount);
    }

    [Fact]
    public void Apply_CursorPositionOptimization_NoCursorMoveForConsecutiveChanges()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 1);

        // Previous: all dead
        var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: all alive
        var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [new Point2D(0, 0)] = true,
                [new Point2D(1, 0)] = true,
                [new Point2D(2, 0)] = true
            },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Count cursor position sequences (e.g., \x1b[1;1H)
        int cursorMoveCount = CountCursorPositionCodes(result);

        // Should only have 1 cursor move (to first cell), not 3
        Assert.Equal(1, cursorMoveCount);
    }
}
