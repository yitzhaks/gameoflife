using BenchmarkDotNet.Attributes;
using GameOfLife.Core;

namespace GameOfLife.Benchmarks.Core;

/// <summary>
/// Benchmarks for RectangularWorld.Tick() - the core simulation step.
/// </summary>
[MemoryDiagnoser]
public class RectangularWorldBenchmarks
{
    private RectangularWorld _world = null!;
    private IGeneration<Point2D, bool> _generation = null!;

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
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _generation?.Dispose();
    }

    [Benchmark]
    public IGeneration<Point2D, bool> Tick()
    {
        var next = _world.Tick(_generation);
        _generation.Dispose();
        _generation = next;
        return next;
    }
}
