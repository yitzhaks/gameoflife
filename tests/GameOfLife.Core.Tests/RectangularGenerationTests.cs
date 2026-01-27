using Xunit;

namespace GameOfLife.Core.Tests;

public class RectangularGenerationTests
{
    [Fact]
    public void Builder_WithValidDimensions_CreatesGeneration()
    {
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateEmptyGeneration((10, 10));

        // Verify we can access cells at the corners
        Assert.False(gen[default]);
        Assert.False(gen[(9, 9)]);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    public void Builder_WithInvalidDimensions_ThrowsArgumentOutOfRange(int width, int height) => Assert.Throws<ArgumentOutOfRangeException>(() => new RectangularGenerationBuilder((width, height)));

    [Fact]
    public void CreateGeneration_WithStates_CopiesStates()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(5, 5)] = true,
        };

        using IGeneration<Point2D, bool> gen = TestHelpers.CreateGeneration((10, 10), states);

        Assert.True(gen[default]);
        Assert.True(gen[(5, 5)]);
    }

    [Fact]
    public void CreateGeneration_WithStatesOutOfBounds_IgnoresOutOfBoundsStates()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(100, 100)] = true, // Out of bounds - should be ignored
        };

        using IGeneration<Point2D, bool> gen = TestHelpers.CreateGeneration((10, 10), states);

        Assert.True(gen[default]);
        // The out-of-bounds point should have been ignored during creation
    }

    [Fact]
    public void Indexer_OutOfBoundsCoordinates_ThrowsArgumentOutOfRange()
    {
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateEmptyGeneration((10, 10));

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => gen[(-1, 0)]);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => gen[(0, -1)]);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => gen[(10, 0)]);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => gen[(0, 10)]);
    }

    [Fact]
    public void Indexer_ValidCoordinates_ReturnsCorrectState()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [(3, 4)] = true,
        };
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateGeneration((10, 10), states);

        Assert.True(gen[(3, 4)]);
        Assert.False(gen[(4, 3)]);
    }

    [Fact]
    public void DefaultState_IsFalse()
    {
        using IGeneration<Point2D, bool> gen = TestHelpers.CreateEmptyGeneration((10, 10));

        // All cells should be dead (false) by default
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                Assert.False(gen[(x, y)]);
            }
        }
    }
}
