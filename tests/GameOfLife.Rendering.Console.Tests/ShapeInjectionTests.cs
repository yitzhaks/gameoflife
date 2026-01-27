using GameOfLife.Console;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ShapeInjectionTests
{
    [Fact]
    public void Parse_ValidFormat_ReturnsShapeInjection()
    {
        var result = ShapeInjection.Parse("glider@5,10");

        Assert.Equal("glider", result.PatternName);
        Assert.Equal(5, result.Position.X);
        Assert.Equal(10, result.Position.Y);
    }

    [Fact]
    public void Parse_NegativeCoordinates_ParsesCorrectly()
    {
        var result = ShapeInjection.Parse("block@-5,-10");

        Assert.Equal("block", result.PatternName);
        Assert.Equal(-5, result.Position.X);
        Assert.Equal(-10, result.Position.Y);
    }

    [Fact]
    public void Parse_ZeroCoordinates_ParsesCorrectly()
    {
        var result = ShapeInjection.Parse("blinker@0,0");

        Assert.Equal("blinker", result.PatternName);
        Assert.Equal(0, result.Position.X);
        Assert.Equal(0, result.Position.Y);
    }

    [Fact]
    public void Parse_PatternWithHyphen_ParsesCorrectly()
    {
        var result = ShapeInjection.Parse("r-pentomino@10,20");

        Assert.Equal("r-pentomino", result.PatternName);
        Assert.Equal(10, result.Position.X);
        Assert.Equal(20, result.Position.Y);
    }

    [Fact]
    public void Parse_MissingAtSign_ThrowsFormatException() => Assert.Throws<FormatException>(() => ShapeInjection.Parse("glider5,10"));

    [Fact]
    public void Parse_MissingComma_ThrowsFormatException() => Assert.Throws<FormatException>(() => ShapeInjection.Parse("glider@510"));

    [Fact]
    public void Parse_InvalidXCoordinate_ThrowsFormatException() => Assert.Throws<FormatException>(() => ShapeInjection.Parse("glider@abc,10"));

    [Fact]
    public void Parse_InvalidYCoordinate_ThrowsFormatException() => Assert.Throws<FormatException>(() => ShapeInjection.Parse("glider@10,xyz"));

    [Fact]
    public void Parse_EmptyPatternName_ThrowsFormatException() => Assert.Throws<FormatException>(() => ShapeInjection.Parse("@5,10"));

    [Fact]
    public void Parse_NullInput_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(() => ShapeInjection.Parse(null!));
}
