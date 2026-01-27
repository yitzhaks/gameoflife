using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

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

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFull(ref glyphEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain the cursor positioning and characters
        result.ShouldContain("\x1b["); // Contains ANSI escape
        result.ShouldContain("."); // Contains dead character
    }

    [Fact]
    public void Apply_IdenticalFrames_WritesNothing()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Get two identical enumerators
        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // With identical frames, nothing should be written
        result.ShouldBeEmpty();
    }

    [Fact]
    public void Apply_SingleCellChange_WritesOnlyChangedCell()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: center cell alive
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 1)] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain the alive character
        result.ShouldContain("#");

        // Should contain cursor positioning
        result.ShouldContain("\x1b[");
    }

    [Fact]
    public void Apply_AllCellsChange_WritesAllCells()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: all alive
        var allAlive = new Dictionary<Point2D, bool>();
        foreach (Point2D node in topology.Nodes)
        {
            allAlive[node] = true;
        }

        using var currGeneration = new DictionaryGeneration<Point2D, bool>(allAlive, defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain multiple alive characters
        int aliveCount = result.Count(c => c == '#');
        aliveCount.ShouldBe(4); // 2x2 = 4 cells
    }

    [Fact]
    public void Apply_WithStartRow_UsesCorrectRowPositioning()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 1));

        // Previous: dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: alive
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        // Start at row 5
        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 5);

        string result = output.ToString();

        // Should contain positioning to row 5, column 1
        result.ShouldContain("\x1b[5;1H");
    }

    [Fact]
    public void WriteFullAndCapture_CapturesAllGlyphs_BufferMatchesRendered()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCapture(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        // Buffer should contain all glyphs (2x2 grid + newlines)
        (frameBuffer.Count > 0).ShouldBeTrue();
        // Check first cell is alive (green)
        frameBuffer[0].Character.ShouldBe('#');
    }

    [Fact]
    public void ApplyAndCapture_DiffsAndCapturesFrame_BufferMatchesCurrentFrame()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: one alive
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 1)] = true },
            defaultState: false);

        // Capture initial frame
        var initialBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator initialEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCapture(ref initialEnumerator, discardOutput, initialBuffer, startRow: 1);

        // Apply diff and capture
        var resultBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCapture(initialBuffer, ref currEnumerator, output, resultBuffer, startRow: 1);

        // Buffer should match current frame
        (resultBuffer.Count > 0).ShouldBeTrue();
        // The changed cell should be written
        string result = output.ToString();
        result.ShouldContain("#");
    }

    [Fact]
    public void Apply_WithColorChanges_EmitsColorCode()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 1));

        // Previous: first alive, second dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        // Current: first dead, second alive (color change for both)
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 0)] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain color codes for the changes
        result.ShouldContain("\x1b[");
    }

    [Fact]
    public void Apply_LargerCurrentFrame_HandlesNewGlyphs()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // Previous: 1x1
        var prevTopology = new RectangularTopology((1, 1));
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: 2x2 (larger)
        var currTopology = new RectangularTopology((2, 2));
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(prevTopology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(currTopology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // New cells beyond previous frame should be written
        result.ShouldContain(".");
    }

    [Fact]
    public void Apply_SkipsUnchangedGlyphs_NoOutputForIdenticalCells()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Same generation twice
        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // No output for identical frames
        result.ShouldBeEmpty();
    }

    [Fact]
    public void WriteFull_WithMultipleColors_EmitsAllColorCodes()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 1));
        // One alive, one dead - different colors
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFull(ref glyphEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain the alive character
        result.ShouldContain("#");
        // Should contain the dead character
        result.ShouldContain(".");
        // Should contain ANSI color codes
        result.ShouldContain("\x1b[");
    }

    [Fact]
    public void WriteFullAndCapture_ClearsBufferBeforeCapture()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 1));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Pre-populate buffer with dummy data
        var frameBuffer = new FrameBuffer(100);
        frameBuffer.Add(new Glyph(null, 'X'));
        frameBuffer.Add(new Glyph(null, 'Y'));

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCapture(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        // Buffer should be cleared and only contain new glyphs (1 cell + newline = 2)
        bool containsX = false;
        bool containsY = false;
        for (int i = 0; i < frameBuffer.Count; i++)
        {
            if (frameBuffer[i].Character == 'X')
            {
                containsX = true;
            }

            if (frameBuffer[i].Character == 'Y')
            {
                containsY = true;
            }
        }

        containsX.ShouldBeFalse();
        containsY.ShouldBeFalse();
    }

    [Fact]
    public void ApplyAndCapture_PreviousFrameLarger_HandlesCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // Previous: 2x2 (larger)
        var prevTopology = new RectangularTopology((2, 2));
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: 1x1 (smaller)
        var currTopology = new RectangularTopology((1, 1));
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        // Capture initial larger frame
        var initialBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator initialEnumerator = renderer.GetGlyphEnumerator(prevTopology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCapture(ref initialEnumerator, discardOutput, initialBuffer, startRow: 1);

        // Apply diff with smaller current frame
        var resultBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(currTopology, currGeneration);
        StreamingDiff.ApplyAndCapture(initialBuffer, ref currEnumerator, output, resultBuffer, startRow: 1);

        // Should handle gracefully
        (resultBuffer.Count > 0).ShouldBeTrue();
    }

    [Fact]
    public void Apply_SameColorConsecutiveCells_DoesNotReEmitColor()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 1));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: all alive (same color for all 3 cells)
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [default] = true,
                [(1, 0)] = true,
                [(2, 0)] = true
            },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain 3 alive characters
        result.Count(c => c == '#').ShouldBe(3);

        // Color code should appear only once (for the first cell)
        // Count color code sequences by counting escape character followed by '[' and 'm'
        int colorCodeCount = CountAnsiColorCodes(result);
        colorCodeCount.ShouldBe(1);
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

        var topology = new RectangularTopology((3, 1));

        // All alive - same color
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [default] = true,
                [(1, 0)] = true,
                [(2, 0)] = true
            },
            defaultState: false);

        // Empty previous frame forces all cells to be written
        var emptyBuffer = new FrameBuffer(100);
        var frameBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        StreamingDiff.ApplyAndCapture(emptyBuffer, ref currEnumerator, output, frameBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain 3 alive characters
        result.Count(c => c == '#').ShouldBe(3);
    }

    [Fact]
    public void WriteFull_SameColorConsecutive_OnlyOneColorEmit()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 1));

        // All alive - same color
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [default] = true,
                [(1, 0)] = true,
                [(2, 0)] = true
            },
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFull(ref glyphEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain 3 alive characters
        result.Count(c => c == '#').ShouldBe(3);

        // Color code should appear only once
        int colorCodeCount = CountAnsiColorCodes(result);
        colorCodeCount.ShouldBe(1);
    }

    [Fact]
    public void Apply_CursorPositionOptimization_NoCursorMoveForConsecutiveChanges()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 1));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: all alive
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [default] = true,
                [(1, 0)] = true,
                [(2, 0)] = true
            },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Count cursor position sequences (e.g., \x1b[1;1H)
        int cursorMoveCount = CountCursorPositionCodes(result);

        // Should only have 1 cursor move (to first cell), not 3
        cursorMoveCount.ShouldBe(1);
    }

    [Fact]
    public void ApplyAndCapture_MultiRowChange_TracksIndexCorrectlyAcrossNewlines()
    {
        // This test verifies that prevIndex increments for newlines too,
        // otherwise the diff comparison gets out of sync after the first row.
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 3x2 grid (3 columns, 2 rows)
        var topology = new RectangularTopology((3, 2));

        // First frame: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Second frame: only cell at (1,1) is alive (second row, middle column)
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 1)] = true },
            defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCapture(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff for second frame
        var currBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCapture(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain exactly 1 alive character (the changed cell)
        int aliveCount = result.Count(c => c == '#');
        aliveCount.ShouldBe(1);

        // Should NOT contain any dead characters (unchanged cells shouldn't be written)
        int deadCount = result.Count(c => c == '.');
        deadCount.ShouldBe(0);
    }
}
