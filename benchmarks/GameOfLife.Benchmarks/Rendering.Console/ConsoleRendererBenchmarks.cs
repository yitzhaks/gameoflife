using BenchmarkDotNet.Attributes;
using GameOfLife.Core;
using GameOfLife.Rendering;
using GameOfLife.Rendering.Console;

namespace GameOfLife.Benchmarks.Rendering.Console;

/// <summary>
/// Benchmarks for ConsoleRenderer - glyph enumeration and cache behavior.
/// </summary>
[MemoryDiagnoser]
public class ConsoleRendererBenchmarks
{
    private RectangularWorld _world = null!;
    private IGeneration<Point2D, bool> _generation = null!;
    private ConsoleRenderer _renderer = null!;
    private Viewport _viewport = null!;

    [Params(50, 100, 1000)]
    public int GridSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Size2D size = (GridSize, GridSize);
        _world = new RectangularWorld(size);

        using var builder = new RectangularGenerationBuilder(size);
        var random = new Random(42);
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (random.NextDouble() < 0.3)
                {
                    builder[(x, y)] = true;
                }
            }
        }
        _generation = builder.Build();

        // Setup renderer with viewport (simulating stress test)
        var layoutEngine = new IdentityLayoutEngine();
        _renderer = new ConsoleRenderer(TextWriter.Null, layoutEngine, ConsoleTheme.Default);
        _viewport = new Viewport(100, 50, GridSize, GridSize);

        // Warm up the renderer cache
        _ = _renderer.GetGlyphEnumerator(_world.Topology, _generation, _viewport);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _generation?.Dispose();
    }

    /// <summary>
    /// Render with warm cache (typical case after first frame).
    /// </summary>
    [Benchmark]
    public int Render()
    {
        var enumerator = _renderer.GetGlyphEnumerator(_world.Topology, _generation, _viewport);
        int count = 0;
        while (enumerator.MoveNext())
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// Render with cold cache (first frame, or cache miss).
    /// This measures HashSet creation for topology.Nodes.
    /// </summary>
    [Benchmark]
    public int RenderColdCache()
    {
        // Create fresh renderer each time (no cache)
        var layoutEngine = new IdentityLayoutEngine();
        var renderer = new ConsoleRenderer(TextWriter.Null, layoutEngine, ConsoleTheme.Default);
        var enumerator = renderer.GetGlyphEnumerator(_world.Topology, _generation, _viewport);
        int count = 0;
        while (enumerator.MoveNext())
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// Full frame render with pre-allocated FrameBuffer.
    /// </summary>
    [Benchmark]
    public int RenderFullFrame()
    {
        var enumerator = _renderer.GetGlyphEnumerator(_world.Topology, _generation, _viewport);
        var frameBuffer = FrameBuffer.ForViewport(_viewport.Width, _viewport.Height);
        StreamingDiff.WriteFullAndCapture(ref enumerator, TextWriter.Null, frameBuffer);
        return frameBuffer.Count;
    }

    /// <summary>
    /// Differential render with pre-allocated FrameBuffer.
    /// </summary>
    [Benchmark]
    public int RenderDiffFrame()
    {
        // Capture first frame
        var enumerator1 = _renderer.GetGlyphEnumerator(_world.Topology, _generation, _viewport);
        var prevBuffer = FrameBuffer.ForViewport(_viewport.Width, _viewport.Height);
        StreamingDiff.WriteFullAndCapture(ref enumerator1, TextWriter.Null, prevBuffer);

        // Diff second frame (same generation, so minimal changes)
        var enumerator2 = _renderer.GetGlyphEnumerator(_world.Topology, _generation, _viewport);
        var currBuffer = FrameBuffer.ForViewport(_viewport.Width, _viewport.Height);
        StreamingDiff.ApplyAndCapture(prevBuffer, ref enumerator2, TextWriter.Null, currBuffer);
        return currBuffer.Count;
    }
}
