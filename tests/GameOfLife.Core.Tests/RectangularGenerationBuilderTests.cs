using Shouldly;

namespace GameOfLife.Core.Tests;

public class RectangularGenerationBuilderTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidSize_CreatesBuilder()
    {
        // Act
        using RectangularGenerationBuilder builder = new((5, 5));

        // Assert
        builder.Size.ShouldBe(new Size2D(5, 5));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    [InlineData(0, 0)]
    [InlineData(-1, -1)]
    public void Constructor_WithInvalidSize_ThrowsArgumentOutOfRangeException(int width, int height) =>
        _ = Should.Throw<ArgumentOutOfRangeException>(() => new RectangularGenerationBuilder((width, height)));

    [Fact]
    public void Constructor_WithClearTrue_InitializesAllCellsToFalse()
    {
        // Arrange & Act
        using RectangularGenerationBuilder builder = new((3, 3), clear: true);

        // Assert - all cells should be false
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                builder[(x, y)].ShouldBeFalse();
            }
        }
    }

    [Fact]
    public void Constructor_WithClearFalse_AllowsExplicitStateSettings()
    {
        // Arrange & Act - clear=false means array is not cleared, but we can still set values
        using RectangularGenerationBuilder builder = new((3, 3), clear: false);

        // Set all values explicitly
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                builder[(x, y)] = (x + y) % 2 == 0;
            }
        }

        // Assert - values should be as set
        builder[(0, 0)].ShouldBeTrue();
        builder[(1, 0)].ShouldBeFalse();
        builder[(0, 1)].ShouldBeFalse();
        builder[(1, 1)].ShouldBeTrue();
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_GetValidCoordinates_ReturnsCorrectState()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        builder[(2, 3)] = true;

        // Act & Assert
        builder[(2, 3)].ShouldBeTrue();
        builder[(0, 0)].ShouldBeFalse();
    }

    [Fact]
    public void Indexer_SetValidCoordinates_UpdatesState()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));

        // Act
        builder[(2, 3)] = true;
        builder[(4, 4)] = true;

        // Assert
        builder[(2, 3)].ShouldBeTrue();
        builder[(4, 4)].ShouldBeTrue();
    }

    [Fact]
    public void Indexer_SetThenClear_UpdatesState()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        builder[(2, 3)] = true;

        // Act
        builder[(2, 3)] = false;

        // Assert
        builder[(2, 3)].ShouldBeFalse();
    }

    [Fact]
    public void Indexer_GetOutOfBoundsCoordinates_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));

        // Act & Assert
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = builder[(-1, 0)]);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = builder[(0, -1)]);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = builder[(5, 0)]);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = builder[(0, 5)]);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = builder[(10, 10)]);
    }

    [Fact]
    public void Indexer_SetOutOfBoundsCoordinates_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));

        // Act & Assert
        _ = Should.Throw<ArgumentOutOfRangeException>(() => builder[(-1, 0)] = true);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => builder[(0, -1)] = true);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => builder[(5, 0)] = true);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => builder[(0, 5)] = true);
    }

    [Fact]
    public void Indexer_SetAfterBuild_ThrowsInvalidOperationException()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Act & Assert
        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => builder[(0, 0)] = true);

        exception.Message.ShouldContain("already been built");
    }

    #endregion

    #region Build Tests

    [Fact]
    public void Build_WithValidBuilder_ReturnsGeneration()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        builder[(2, 3)] = true;

        // Act
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Assert
        generation[(2, 3)].ShouldBeTrue();
        generation[(0, 0)].ShouldBeFalse();
    }

    [Fact]
    public void Build_CalledTwice_ThrowsObjectDisposedException()
    {
        // Arrange - Build() transfers ownership, so builder becomes disposed-like state
        using RectangularGenerationBuilder builder = new((5, 5));
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Act & Assert - since _states is set to null after Build(), ObjectDisposedException is thrown
        // Note: This tests the ObjectDisposedException path. The InvalidOperationException check for
        // "_built == true" in Build() (lines 68-70) is unreachable dead code because:
        // 1. Build() sets _states = null right after setting _built = true
        // 2. The ObjectDisposedException.ThrowIf(_states == null, this) check happens BEFORE the _built check
        // 3. Therefore, if _built is true, _states is guaranteed to be null, and ObjectDisposedException
        //    will always be thrown before reaching the _built check
        // This defensive code exists for safety if the implementation changes but cannot be tested
        // with the current implementation.
        _ = Should.Throw<ObjectDisposedException>(builder.Build);
    }

    [Fact]
    public void Build_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        builder.Dispose();

        // Act & Assert
        _ = Should.Throw<ObjectDisposedException>(builder.Build);
    }

    [Fact]
    public void Build_TransfersOwnershipOfArray()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((3, 3));
        builder[(1, 1)] = true;

        // Act
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Assert - generation should have the state we set
        generation[(1, 1)].ShouldBeTrue();

        // Verify builder cannot be used after Build
        _ = Should.Throw<InvalidOperationException>(() => builder[(0, 0)] = true);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_BeforeBuild_ReturnsArrayToPool()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        builder[(2, 3)] = true;

        // Act - dispose without calling Build
        builder.Dispose();

        // Assert - Build should fail because array was returned to pool
        _ = Should.Throw<ObjectDisposedException>(builder.Build);
    }

    [Fact]
    public void Dispose_AfterBuild_DoesNotThrow()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Act & Assert - should not throw
        Should.NotThrow(builder.Dispose);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));

        // Act & Assert - multiple dispose calls should be safe
        Should.NotThrow(() =>
        {
            builder.Dispose();
            builder.Dispose();
            builder.Dispose();
        });
    }

    [Fact]
    public void Dispose_AfterBuildCalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Act & Assert - multiple dispose calls after build should be safe
        Should.NotThrow(() =>
        {
            builder.Dispose();
            builder.Dispose();
        });
    }

    #endregion

    #region Generation (built result) Tests

    [Fact]
    public void Generation_Indexer_OutOfBounds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Act & Assert
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = generation[(-1, 0)]);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = generation[(0, -1)]);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = generation[(5, 0)]);
        _ = Should.Throw<ArgumentOutOfRangeException>(() => _ = generation[(0, 5)]);
    }

    [Fact]
    public void Generation_Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((5, 5));
        IGeneration<Point2D, bool> generation = builder.Build();

        // Act & Assert - multiple dispose calls should be safe
        Should.NotThrow(() =>
        {
            generation.Dispose();
            generation.Dispose();
            generation.Dispose();
        });
    }

    [Fact]
    public void Generation_PreservesAllSetStates()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((10, 10));
        HashSet<Point2D> expectedAlive =
        [
            (0, 0),
            (5, 5),
            (9, 9),
            (3, 7),
        ];

        foreach (Point2D point in expectedAlive)
        {
            builder[point] = true;
        }

        // Act
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Assert
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                Point2D point = (x, y);
                if (expectedAlive.Contains(point))
                {
                    generation[point].ShouldBeTrue($"Expected ({x}, {y}) to be alive");
                }
                else
                {
                    generation[point].ShouldBeFalse($"Expected ({x}, {y}) to be dead");
                }
            }
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithMinimumSize_Succeeds()
    {
        // Arrange & Act
        using RectangularGenerationBuilder builder = new((1, 1));

        // Assert
        builder.Size.ShouldBe(new Size2D(1, 1));
        builder[(0, 0)].ShouldBeFalse();
    }

    [Fact]
    public void Build_With1x1Grid_ReturnsValidGeneration()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((1, 1));
        builder[(0, 0)] = true;

        // Act
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Assert
        generation[(0, 0)].ShouldBeTrue();
    }

    [Fact]
    public void Build_WithLargeGrid_Succeeds()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((100, 100));
        builder[(50, 50)] = true;
        builder[(99, 99)] = true;

        // Act
        using IGeneration<Point2D, bool> generation = builder.Build();

        // Assert
        generation[(50, 50)].ShouldBeTrue();
        generation[(99, 99)].ShouldBeTrue();
        generation[(0, 0)].ShouldBeFalse();
    }

    [Fact]
    public void Indexer_CornerCoordinates_WorkCorrectly()
    {
        // Arrange
        using RectangularGenerationBuilder builder = new((10, 10));

        // Act - set all corners
        builder[(0, 0)] = true;
        builder[(9, 0)] = true;
        builder[(0, 9)] = true;
        builder[(9, 9)] = true;

        // Assert
        builder[(0, 0)].ShouldBeTrue();
        builder[(9, 0)].ShouldBeTrue();
        builder[(0, 9)].ShouldBeTrue();
        builder[(9, 9)].ShouldBeTrue();
        builder[(5, 5)].ShouldBeFalse(); // center should be false
    }

    [Fact]
    public void Size_ReturnsCorrectDimensions()
    {
        // Arrange & Act
        using RectangularGenerationBuilder builder1 = new((5, 10));
        using RectangularGenerationBuilder builder2 = new((10, 5));

        // Assert
        builder1.Size.Width.ShouldBe(5);
        builder1.Size.Height.ShouldBe(10);
        builder2.Size.Width.ShouldBe(10);
        builder2.Size.Height.ShouldBe(5);
    }

    #endregion
}
