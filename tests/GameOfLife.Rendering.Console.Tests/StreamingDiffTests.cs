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

        var glyphEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        StreamingDiff.WriteFull(ref glyphEnumerator, output, startRow: 1);

        var result = output.ToString();

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
        var prevEnumerator = renderer.GetGlyphEnumerator(topology, generation);
        var currEnumerator = renderer.GetGlyphEnumerator(topology, generation);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        var result = output.ToString();

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

        var prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        var currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        var result = output.ToString();

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
        foreach (var node in topology.Nodes)
        {
            allAlive[node] = true;
        }

        var currGeneration = new DictionaryGeneration<Point2D, bool>(allAlive, defaultState: false);

        var prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        var currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 1);

        var result = output.ToString();

        // Should contain multiple alive characters
        var aliveCount = result.Count(c => c == '#');
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

        var prevEnumerator = renderer.GetGlyphEnumerator(topology, prevGeneration);
        var currEnumerator = renderer.GetGlyphEnumerator(topology, currGeneration);

        // Start at row 5
        StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, output, startRow: 5);

        var result = output.ToString();

        // Should contain positioning to row 5, column 1
        Assert.Contains("\x1b[5;1H", result);
    }
}
