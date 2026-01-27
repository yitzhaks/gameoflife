using Shouldly;

namespace GameOfLife.Core.Tests;

public class Array2DTests
{
    #region ReadOnlyArray2D Tests

    [Fact]
    public void ReadOnlyArray2D_Constructor_ArrayTooSmall_ThrowsArgumentException()
    {
        // Arrange
        int[] data = new int[5]; // Too small for 3x3
        Size2D size = new(3, 3); // Requires 9 elements

        // Act & Assert
        try
        {
            _ = new ReadOnlyArray2D<int>(data, size);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException ex)
        {
            ex.ParamName.ShouldBe("data");
            ex.Message.ShouldContain("5");
            ex.Message.ShouldContain("9");
        }
    }

    [Fact]
    public void ReadOnlyArray2D_Constructor_ArrayExactSize_Succeeds()
    {
        // Arrange
        int[] data = new int[9];
        Size2D size = new(3, 3);

        // Act
        var array = new ReadOnlyArray2D<int>(data, size);

        // Assert
        array.Size.ShouldBe(size);
    }

    [Fact]
    public void ReadOnlyArray2D_Constructor_ArrayLargerThanRequired_Succeeds()
    {
        // Arrange
        int[] data = new int[20]; // Larger than needed
        Size2D size = new(3, 3); // Requires 9 elements

        // Act
        var array = new ReadOnlyArray2D<int>(data, size);

        // Assert
        array.Size.ShouldBe(size);
    }

    [Fact]
    public void ReadOnlyArray2D_Indexer_OutOfBounds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new ReadOnlyArray2D<int>(data, size);
        Point2D outOfBounds = (5, 5);

        // Act & Assert
        try
        {
            _ = array[outOfBounds];
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ex.ParamName.ShouldBe("point");
            ex.Message.ShouldContain("5");
            ex.Message.ShouldContain("3x3");
        }
    }

    [Fact]
    public void ReadOnlyArray2D_Indexer_NegativeCoordinate_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new ReadOnlyArray2D<int>(data, size);
        Point2D negative = (-1, 0);

        // Act & Assert
        try
        {
            _ = array[negative];
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected
        }
    }

    [Fact]
    public void ReadOnlyArray2D_Indexer_ValidCoordinate_ReturnsValue()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new ReadOnlyArray2D<int>(data, size);

        // Act & Assert
        array[(0, 0)].ShouldBe(1);
        array[(1, 0)].ShouldBe(2);
        array[(2, 0)].ShouldBe(3);
        array[(0, 1)].ShouldBe(4);
        array[(1, 1)].ShouldBe(5);
        array[(2, 2)].ShouldBe(9);
    }

    [Fact]
    public void ReadOnlyArray2D_GetOrDefault_ValidCoordinate_ReturnsValue()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new ReadOnlyArray2D<int>(data, size);

        // Act & Assert
        array.GetOrDefault((1, 1)).ShouldBe(5);
        array.GetOrDefault((0, 0)).ShouldBe(1);
    }

    [Fact]
    public void ReadOnlyArray2D_GetOrDefault_OutOfBounds_ReturnsDefaultValue()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new ReadOnlyArray2D<int>(data, size);

        // Act & Assert
        array.GetOrDefault((5, 5)).ShouldBe(0); // default(int) is 0
        array.GetOrDefault((-1, 0)).ShouldBe(0);
    }

    [Fact]
    public void ReadOnlyArray2D_GetOrDefault_OutOfBounds_ReturnsSpecifiedDefault()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new ReadOnlyArray2D<int>(data, size);

        // Act & Assert
        array.GetOrDefault((5, 5), 42).ShouldBe(42);
        array.GetOrDefault((-1, 0), -1).ShouldBe(-1);
    }

    #endregion

    #region Array2D Tests

    [Fact]
    public void Array2D_Constructor_ArrayTooSmall_ThrowsArgumentException()
    {
        // Arrange
        int[] data = new int[5]; // Too small for 3x3
        Size2D size = new(3, 3); // Requires 9 elements

        // Act & Assert
        try
        {
            _ = new Array2D<int>(data, size);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException ex)
        {
            ex.ParamName.ShouldBe("data");
            ex.Message.ShouldContain("5");
            ex.Message.ShouldContain("9");
        }
    }

    [Fact]
    public void Array2D_Constructor_ArrayExactSize_Succeeds()
    {
        // Arrange
        int[] data = new int[9];
        Size2D size = new(3, 3);

        // Act
        var array = new Array2D<int>(data, size);

        // Assert
        array.Size.ShouldBe(size);
    }

    [Fact]
    public void Array2D_Constructor_ArrayLargerThanRequired_Succeeds()
    {
        // Arrange
        int[] data = new int[20]; // Larger than needed
        Size2D size = new(3, 3); // Requires 9 elements

        // Act
        var array = new Array2D<int>(data, size);

        // Assert
        array.Size.ShouldBe(size);
    }

    [Fact]
    public void Array2D_Indexer_OutOfBounds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new Array2D<int>(data, size);
        Point2D outOfBounds = (5, 5);

        // Act & Assert
        try
        {
            _ = array[outOfBounds];
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ex.ParamName.ShouldBe("point");
            ex.Message.ShouldContain("5");
            ex.Message.ShouldContain("3x3");
        }
    }

    [Fact]
    public void Array2D_Indexer_NegativeCoordinate_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new Array2D<int>(data, size);
        Point2D negative = (-1, 0);

        // Act & Assert
        try
        {
            _ = array[negative];
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected
        }
    }

    [Fact]
    public void Array2D_Indexer_ValidCoordinate_ReturnsValue()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new Array2D<int>(data, size);

        // Act & Assert
        array[(0, 0)].ShouldBe(1);
        array[(1, 0)].ShouldBe(2);
        array[(2, 0)].ShouldBe(3);
        array[(0, 1)].ShouldBe(4);
        array[(1, 1)].ShouldBe(5);
        array[(2, 2)].ShouldBe(9);
    }

    [Fact]
    public void Array2D_Indexer_CanSetValue()
    {
        // Arrange
        int[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        Size2D size = new(3, 3);
        var array = new Array2D<int>(data, size);

        // Act
        array[(1, 1)] = 99;

        // Assert
        array[(1, 1)].ShouldBe(99);
        data[4].ShouldBe(99); // Verify underlying array was modified
    }

    #endregion
}
