using Xunit;

namespace GameOfLife.Core.Tests;

public class RectangularWorldTests
{
    [Fact]
    public void Constructor_WithValidSize_CreatesWorld()
    {
        var world = new RectangularWorld((10, 10));

        Assert.Equal(new Size2D(10, 10), world.Size);
        Assert.NotNull(world.Topology);
    }

    [Fact]
    public void Tick_WithNullGeneration_ThrowsArgumentNull()
    {
        var world = new RectangularWorld((10, 10));

        _ = Assert.Throws<ArgumentNullException>(() => world.Tick(null!));
    }

    [Fact]
    public void Tick_EmptyGrid_StaysEmpty()
    {
        var world = new RectangularWorld((5, 5));
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateEmptyGeneration((5, 5));

        using IGeneration<Point2D, bool> next = world.Tick(gen);

        // All cells should remain dead
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.False(next[(x, y)]);
            }
        }
    }

    [Fact]
    public void Tick_SingleCell_Dies()
    {
        var world = new RectangularWorld((5, 5));
        var states = new Dictionary<Point2D, bool>
        {
            [(2, 2)] = true,
        };
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateGeneration((5, 5), states);

        using IGeneration<Point2D, bool> next = world.Tick(gen);

        // Single cell dies from underpopulation
        Assert.False(next[(2, 2)]);
    }

    [Fact]
    public void Tick_Block_StaysStable()
    {
        // Block is a stable pattern (still life)
        // ##
        // ##
        var world = new RectangularWorld((5, 5));
        var states = new Dictionary<Point2D, bool>
        {
            [(1, 1)] = true,
            [(2, 1)] = true,
            [(1, 2)] = true,
            [(2, 2)] = true,
        };
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateGeneration((5, 5), states);

        using IGeneration<Point2D, bool> next = world.Tick(gen);

        Assert.True(next[(1, 1)]);
        Assert.True(next[(2, 1)]);
        Assert.True(next[(1, 2)]);
        Assert.True(next[(2, 2)]);
    }

    [Fact]
    public void Tick_Blinker_Oscillates()
    {
        // Blinker oscillates between horizontal and vertical
        // Phase 1: ###  Phase 2: #
        //                        #
        //                        #
        var world = new RectangularWorld((5, 5));
        var states = new Dictionary<Point2D, bool>
        {
            [(1, 2)] = true,
            [(2, 2)] = true,
            [(3, 2)] = true,
        };
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateGeneration((5, 5), states);

        using IGeneration<Point2D, bool> next = world.Tick(gen);

        // Should be vertical now
        Assert.True(next[(2, 1)]);
        Assert.True(next[(2, 2)]);
        Assert.True(next[(2, 3)]);
        Assert.False(next[(1, 2)]);
        Assert.False(next[(3, 2)]);

        using IGeneration<Point2D, bool> next2 = world.Tick(next);

        // Should be back to horizontal
        Assert.True(next2[(1, 2)]);
        Assert.True(next2[(2, 2)]);
        Assert.True(next2[(3, 2)]);
        Assert.False(next2[(2, 1)]);
        Assert.False(next2[(2, 3)]);
    }

    [Fact]
    public void Tick_Birth_WithThreeNeighbors()
    {
        // Three cells in an L pattern should give birth to a fourth
        //  #
        //  ##
        var world = new RectangularWorld((5, 5));
        var states = new Dictionary<Point2D, bool>
        {
            [(2, 1)] = true,
            [(2, 2)] = true,
            [(3, 2)] = true,
        };
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateGeneration((5, 5), states);

        using IGeneration<Point2D, bool> next = world.Tick(gen);

        // Cell at (3,1) should be born (has exactly 3 neighbors)
        Assert.True(next[(3, 1)]);
    }

    [Fact]
    public void Tick_Overpopulation_CellDies()
    {
        // Cell with 4+ neighbors dies
        //  #
        // ###
        //  #
        var world = new RectangularWorld((5, 5));
        var states = new Dictionary<Point2D, bool>
        {
            [(2, 1)] = true,
            [(1, 2)] = true,
            [(2, 2)] = true,
            [(3, 2)] = true,
            [(2, 3)] = true,
        };
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateGeneration((5, 5), states);

        using IGeneration<Point2D, bool> next = world.Tick(gen);

        // Center cell should die from overpopulation
        Assert.False(next[(2, 2)]);
    }

    [Fact]
    public void Tick_MatchesOriginalWorldBehavior()
    {
        // Verify RectangularWorld produces same results as World<Point2D, bool>
        var topology = new RectangularTopology((10, 10));
        var world = new World<Point2D, bool>(topology, new ClassicRules());
        var rectangularWorld = new RectangularWorld((10, 10));

        // Create deterministic initial state (same as using Random(42) seed)
        var states = new Dictionary<Point2D, bool>();
#pragma warning disable CA5394 // Do not use insecure randomness - this is a test with a fixed seed for determinism
        var random = new Random(42);
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (random.NextDouble() < 0.3)
                {
                    states[(x, y)] = true;
                }
            }
        }
#pragma warning restore CA5394

        IGeneration<Point2D, bool> dictGen = new DictionaryGeneration<Point2D, bool>(states, false);
        IGeneration<Point2D, bool> arrayGen = TestHelpers.CreateGeneration((10, 10), states);

        // Run 10 generations and compare
        for (int i = 0; i < 10; i++)
        {
            IGeneration<Point2D, bool> prevDictGen = dictGen;
            dictGen = world.Tick(dictGen);
            prevDictGen.Dispose();

            IGeneration<Point2D, bool> prevArrayGen = arrayGen;
            arrayGen = rectangularWorld.Tick(arrayGen);
            prevArrayGen.Dispose();

            // Compare all cells
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Point2D point = (x, y);
                    Assert.Equal(dictGen[point], arrayGen[point]);
                }
            }
        }

        dictGen.Dispose();
        arrayGen.Dispose();
    }
}
