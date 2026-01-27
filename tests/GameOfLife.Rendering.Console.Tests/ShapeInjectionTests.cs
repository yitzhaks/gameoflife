using GameOfLife.Console;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ShapeInjectionTests
{
    [Fact]
    public void Parse_ValidFormat_ReturnsShapeInjection()
    {
        var result = ShapeInjection.Parse("glider@5,10");

        result.PatternName.ShouldBe("glider");
        result.Position.X.ShouldBe(5);
        result.Position.Y.ShouldBe(10);
    }

    [Fact]
    public void Parse_NegativeCoordinates_ParsesCorrectly()
    {
        var result = ShapeInjection.Parse("block@-5,-10");

        result.PatternName.ShouldBe("block");
        result.Position.X.ShouldBe(-5);
        result.Position.Y.ShouldBe(-10);
    }

    [Fact]
    public void Parse_ZeroCoordinates_ParsesCorrectly()
    {
        var result = ShapeInjection.Parse("blinker@0,0");

        result.PatternName.ShouldBe("blinker");
        result.Position.X.ShouldBe(0);
        result.Position.Y.ShouldBe(0);
    }

    [Fact]
    public void Parse_PatternWithHyphen_ParsesCorrectly()
    {
        var result = ShapeInjection.Parse("r-pentomino@10,20");

        result.PatternName.ShouldBe("r-pentomino");
        result.Position.X.ShouldBe(10);
        result.Position.Y.ShouldBe(20);
    }

    [Fact]
    public void Parse_MissingAtSign_ThrowsFormatException() => Should.Throw<FormatException>(() => ShapeInjection.Parse("glider5,10"));

    [Fact]
    public void Parse_MissingComma_ThrowsFormatException() => Should.Throw<FormatException>(() => ShapeInjection.Parse("glider@510"));

    [Fact]
    public void Parse_InvalidXCoordinate_ThrowsFormatException() => Should.Throw<FormatException>(() => ShapeInjection.Parse("glider@abc,10"));

    [Fact]
    public void Parse_InvalidYCoordinate_ThrowsFormatException() => Should.Throw<FormatException>(() => ShapeInjection.Parse("glider@10,xyz"));

    [Fact]
    public void Parse_EmptyPatternName_ThrowsFormatException() => Should.Throw<FormatException>(() => ShapeInjection.Parse("@5,10"));

    [Fact]
    public void Parse_NullInput_ThrowsArgumentNullException() => Should.Throw<ArgumentNullException>(() => ShapeInjection.Parse(null!));
}
