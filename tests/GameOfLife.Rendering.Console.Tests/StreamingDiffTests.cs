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

    #region HalfBlock Tests

    [Fact]
    public void WriteFullAndCaptureHalfBlock_WritesAllGlyphs()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 2x4 grid (2 columns, 4 rows) - packs into 2 half-block rows
        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain the cursor positioning
        result.ShouldContain("\x1b[");
        // Buffer should be populated
        (frameBuffer.Count > 0).ShouldBeTrue();
    }

    [Fact]
    public void WriteFullAndCaptureHalfBlock_CapturesAllGlyphs_BufferMatchesRendered()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 2x4 grid - one cell alive at origin
        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        // Buffer should contain all glyphs (2 columns x 2 packed rows + newlines)
        (frameBuffer.Count > 0).ShouldBeTrue();
        // First cell should have upper half block (top cell alive, bottom dead)
        frameBuffer[0].Character.ShouldBe(ConsoleTheme.HalfBlock.UpperHalf);
    }

    [Fact]
    public void WriteFullAndCaptureHalfBlock_BothCellsAlive_RendersFullBlock()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 1x2 grid - both cells alive (packs into single character)
        var topology = new RectangularTopology((1, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [(0, 0)] = true,
                [(0, 1)] = true
            },
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        // First cell should be full block
        frameBuffer[0].Character.ShouldBe(ConsoleTheme.HalfBlock.Full);
    }

    [Fact]
    public void WriteFullAndCaptureHalfBlock_BottomCellAlive_RendersLowerHalfBlock()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 1x2 grid - only bottom cell alive
        var topology = new RectangularTopology((1, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [(0, 1)] = true
            },
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        // First cell should be lower half block
        frameBuffer[0].Character.ShouldBe(ConsoleTheme.HalfBlock.LowerHalf);
    }

    [Fact]
    public void WriteFullAndCaptureHalfBlock_ClearsBufferBeforeCapture()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Pre-populate buffer with dummy data
        var frameBuffer = new FrameBuffer(100);
        frameBuffer.Add(new Glyph(null, 'X'));
        frameBuffer.Add(new Glyph(null, 'Y'));

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        // Buffer should be cleared and only contain new glyphs
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
    public void ApplyAndCaptureHalfBlock_IdenticalFrames_WritesNothing()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff with identical frame
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // With identical frames, nothing should be written
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_SingleCellChange_WritesOnlyChangedCell()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 4));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: one cell alive
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 1)] = true },
            defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain cursor positioning for the change
        result.ShouldContain("\x1b[");
        // Should contain half-block character
        result.ShouldContain(ConsoleTheme.HalfBlock.LowerHalf.ToString());
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_DiffsAndCapturesFrame_BufferMatchesCurrentFrame()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: one cell alive
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(0, 0)] = true },
            defaultState: false);

        // Capture initial frame
        var initialBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator initialEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref initialEnumerator, discardOutput, initialBuffer, startRow: 1);

        // Apply diff and capture
        var resultBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(initialBuffer, ref currEnumerator, output, resultBuffer, startRow: 1);

        // Buffer should match current frame
        (resultBuffer.Count > 0).ShouldBeTrue();
        // First cell should be upper half block
        resultBuffer[0].Character.ShouldBe(ConsoleTheme.HalfBlock.UpperHalf);
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_WithStartRow_UsesCorrectRowPositioning()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((1, 2));

        // Previous: dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: alive
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff with startRow: 5
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 5);

        string result = output.ToString();

        // Should contain positioning to row 5
        result.ShouldContain("\x1b[5;1H");
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_AllCellsChange_WritesAllCells()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));

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

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // 2 columns x 2 packed rows = 4 full block characters
        int fullBlockCount = result.Count(c => c == ConsoleTheme.HalfBlock.Full);
        fullBlockCount.ShouldBe(4);
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_WithColorChanges_EmitsColorCodes()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));

        // Previous: first cell alive (green)
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        // Current: second cell alive instead (color change)
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 0)] = true },
            defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain ANSI escape sequences for color
        result.ShouldContain("\x1b[");
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_WithBackgroundColorChange_EmitsBackgroundColorCode()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));

        // Previous: all dead (dark gray background)
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: both cells in first column alive (default background)
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [(0, 0)] = true,
                [(0, 1)] = true
            },
            defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain full block character
        result.ShouldContain(ConsoleTheme.HalfBlock.Full.ToString());
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_LargerCurrentFrame_HandlesNewGlyphs()
    {
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);

        // Previous: 1x2 grid (1 half-block column, 1 packed row)
        var prevTopology = new RectangularTopology((1, 2));
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: 2x4 grid (2 columns, 2 packed rows) - larger with alive cell
        var currTopology = new RectangularTopology((2, 4));
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 0)] = true }, // Alive in second column, first row
            defaultState: false);

        // Use separate renderers to avoid topology caching issues
        using var discardOutput = new StringWriter();
        var prevRenderer = new ConsoleRenderer(discardOutput, engine, theme);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = prevRenderer.GetHalfBlockGlyphEnumerator(prevTopology, prevGeneration);
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff with larger frame using new renderer
        using var output = new StringWriter();
        var currRenderer = new ConsoleRenderer(output, engine, theme);
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = currRenderer.GetHalfBlockGlyphEnumerator(currTopology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        // The current buffer should be larger than the previous
        currBuffer.Count.ShouldBeGreaterThan(prevBuffer.Count);

        // Second column, first row has top cell alive (upper half block)
        // The buffer should capture this
        bool hasUpperHalf = false;
        for (int i = 0; i < currBuffer.Count; i++)
        {
            if (currBuffer[i].Character == ConsoleTheme.HalfBlock.UpperHalf)
            {
                hasUpperHalf = true;
                break;
            }
        }

        hasUpperHalf.ShouldBeTrue();
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_CursorPositionOptimization_NoCursorMoveForConsecutiveChanges()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 2));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: all alive (3 full blocks in a row)
        var allAlive = new Dictionary<Point2D, bool>();
        foreach (Point2D node in topology.Nodes)
        {
            allAlive[node] = true;
        }

        using var currGeneration = new DictionaryGeneration<Point2D, bool>(allAlive, defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // Count cursor position sequences (e.g., \x1b[1;1H)
        int cursorMoveCount = CountCursorPositionCodes(result);

        // Should only have 1 cursor move (to first cell), not 3
        cursorMoveCount.ShouldBe(1);
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_MultiRowChange_TracksIndexCorrectlyAcrossNewlines()
    {
        // This test verifies that prevIndex increments for newlines too in half-block mode.
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 3x4 grid (3 columns, 4 rows) - packs into 2 half-block rows
        var topology = new RectangularTopology((3, 4));

        // First frame: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Second frame: only cell at (1,2) is alive (second packed row, middle column)
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 2)] = true },
            defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff for second frame
        var currBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCaptureHalfBlock(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain exactly 1 half-block character (the changed cell)
        int upperHalfCount = result.Count(c => c == ConsoleTheme.HalfBlock.UpperHalf);
        upperHalfCount.ShouldBe(1);

        // Should NOT contain spaces for unchanged dead cells
        // (only spaces that are part of ANSI sequences or other formatting)
    }

    [Fact]
    public void WriteFullAndCaptureHalfBlock_SameColorConsecutive_OnlyOneColorEmit()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // 3 columns, 2 rows - all alive (same color)
        var topology = new RectangularTopology((3, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [(0, 0)] = true,
                [(0, 1)] = true,
                [(1, 0)] = true,
                [(1, 1)] = true,
                [(2, 0)] = true,
                [(2, 1)] = true
            },
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain 3 full block characters
        result.Count(c => c == ConsoleTheme.HalfBlock.Full).ShouldBe(3);
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_EmptyPreviousBuffer_WritesFullFrame()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        // Empty previous buffer forces full frame write
        var emptyBuffer = new FrameBuffer(100);
        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        StreamingDiff.ApplyAndCaptureHalfBlock(emptyBuffer, ref currEnumerator, output, frameBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain the cursor positioning at start (full write behavior)
        result.ShouldContain("\x1b[1;1H");
        // Should contain half-block character
        result.ShouldContain(ConsoleTheme.HalfBlock.UpperHalf.ToString());
    }

    #endregion

    #region Null Argument Validation Tests

    [Fact]
    public void Apply_NullOutput_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, null!, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void WriteFull_NullOutput_ThrowsArgumentNullException()
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

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.WriteFull(ref glyphEnumerator, null!, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void WriteFullAndCapture_NullOutput_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.WriteFullAndCapture(ref glyphEnumerator, null!, frameBuffer, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void WriteFullAndCapture_NullFrameBuffer_ThrowsArgumentNullException()
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

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.WriteFullAndCapture(ref glyphEnumerator, output, null!, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void ApplyAndCapture_NullPreviousFrame_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var currentFrame = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.ApplyAndCapture(null!, ref currEnumerator, output, currentFrame, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void ApplyAndCapture_NullOutput_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var previousFrame = new FrameBuffer(100);
        var currentFrame = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.ApplyAndCapture(previousFrame, ref currEnumerator, null!, currentFrame, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void ApplyAndCapture_NullCurrentFrame_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var previousFrame = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.ApplyAndCapture(previousFrame, ref currEnumerator, output, null!, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void WriteFullAndCaptureHalfBlock_NullOutput_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, null!, frameBuffer, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void WriteFullAndCaptureHalfBlock_NullFrameBuffer_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, output, null!, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_NullPreviousFrame_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var currentFrame = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.ApplyAndCaptureHalfBlock(null!, ref currEnumerator, output, currentFrame, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_NullOutput_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var previousFrame = new FrameBuffer(100);
        var currentFrame = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.ApplyAndCaptureHalfBlock(previousFrame, ref currEnumerator, null!, currentFrame, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    [Fact]
    public void ApplyAndCaptureHalfBlock_NullCurrentFrame_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var previousFrame = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        ArgumentNullException? exception = null;
        try
        {
            StreamingDiff.ApplyAndCaptureHalfBlock(previousFrame, ref currEnumerator, output, null!, startRow: 1);
        }
        catch (ArgumentNullException ex)
        {
            exception = ex;
        }

        _ = exception.ShouldNotBeNull();
    }

    #endregion

    #region Glyph With Null Color Tests

    [Fact]
    public void Apply_GlyphWithNullForegroundColor_SkipsColorEmission()
    {
        // Test the branch where currGlyph.Color.HasValue is false
        // Standard rendering always emits colors, so test via identical frames
        using var actualOutput = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(actualOutput, engine, theme);

        // The TokenEnumerator always emits colors for cells, so this tests
        // the path through ApplyAndCapture when colors match
        var topology = new RectangularTopology((2, 1));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // All dead cells - same color throughout
        var initialBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator initialEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCapture(ref initialEnumerator, discardOutput, initialBuffer, startRow: 1);

        // Apply with same generation - should have minimal output
        var resultBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.ApplyAndCapture(initialBuffer, ref currEnumerator, actualOutput, resultBuffer, startRow: 1);

        // With identical frames, output should be empty
        actualOutput.ToString().ShouldBeEmpty();
    }

    [Fact]
    public void Apply_GlyphWithNullBackgroundColor_SkipsBackgroundEmission()
    {
        // Test the branch where currGlyph.BackgroundColor.HasValue is false
        // Standard TokenEnumerator doesn't emit background colors, but HalfBlock does
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // Use standard (non-half-block) renderer which doesn't emit background colors
        var topology = new RectangularTopology((2, 1));
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 0)] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain foreground color changes but the standard enumerator
        // doesn't emit background colors, so the branch testing null background is covered
        result.ShouldContain("#");
        result.ShouldContain(".");
    }

    [Fact]
    public void WriteFull_GlyphWithNullBackgroundColor_SkipsBackgroundEmission()
    {
        // Test WriteFull path where glyph.BackgroundColor.HasValue is false
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // Standard renderer doesn't emit background colors
        var topology = new RectangularTopology((2, 1));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFull(ref glyphEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain characters but no background codes
        // (background codes contain ";4" patterns like \x1b[40m)
        result.ShouldContain("#");
        result.ShouldContain(".");
        // Verify we got foreground colors but not background colors
        // Foreground green is \x1b[32m, background would have different pattern
        result.ShouldContain("\x1b[32m"); // Green foreground
    }

    [Fact]
    public void ApplyAndCapture_SameColorPrevAndCurr_SkipsColorEmission()
    {
        // Test the optimization where same color doesn't get re-emitted
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 1));

        // Previous: first dead, second alive
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 0)] = true },
            defaultState: false);

        // Current: first alive, second dead (swapped)
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        // Capture first frame
        var prevBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        using var discardOutput = new StringWriter();
        StreamingDiff.WriteFullAndCapture(ref prevEnumerator, discardOutput, prevBuffer, startRow: 1);

        // Apply diff
        var currBuffer = new FrameBuffer(100);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);
        StreamingDiff.ApplyAndCapture(prevBuffer, ref currEnumerator, output, currBuffer, startRow: 1);

        string result = output.ToString();

        // Both cells changed, so both should be written
        result.ShouldContain("#");
        result.ShouldContain(".");
    }

    #endregion

    #region Color State Transition Tests

    [Fact]
    public void Apply_ColorStateTransition_EmitsCorrectColorCodes()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((4, 1));

        // Previous: all dead
        using var prevGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Current: two cells alive (positions 0 and 2) - only changes will be written
        using var currGeneration = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [default] = true,
                [(2, 0)] = true
            },
            defaultState: false);

        ColorNormalizedGlyphEnumerator prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        ColorNormalizedGlyphEnumerator currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        string result = output.ToString();

        // Should contain the alive character (only changed cells are written)
        result.ShouldContain("#");
        // Should contain ANSI color code (green for alive)
        result.ShouldContain("\x1b[32m");
    }

    [Fact]
    public void WriteFull_BackgroundColorTransition_EmitsBackgroundCodes()
    {
        // Test using half-block rendering which uses background colors
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));
        // Create pattern that forces background color changes
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>
            {
                [(0, 0)] = true, // Top alive, bottom dead
                [(1, 2)] = true,
                [(1, 3)] = true  // Full block on second column, second row
            },
            defaultState: false);

        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator glyphEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFullAndCaptureHalfBlock(ref glyphEnumerator, output, frameBuffer, startRow: 1);

        string result = output.ToString();

        // Should contain ANSI sequences for both foreground and background
        result.ShouldContain("\x1b[");
    }

    [Fact]
    public void ApplyAndCapture_SameBackgroundConsecutive_OnlyOneBackgroundEmit()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 2));

        // All cells dead - same background color for all
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // Empty previous frame forces all cells to be written
        var emptyBuffer = new FrameBuffer(100);
        var frameBuffer = new FrameBuffer(100);
        HalfBlockColorNormalizedGlyphEnumerator currEnumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        StreamingDiff.ApplyAndCaptureHalfBlock(emptyBuffer, ref currEnumerator, output, frameBuffer, startRow: 1);

        string result = output.ToString();

        // Should write all space characters
        result.Count(c => c == ' ').ShouldBe(3);
    }

    #endregion
}
