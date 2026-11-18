using GameOfLife.Console;
using GameOfLife.Core;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class HexShapeInjectionTests
{
    #region Parse Valid Input Tests

    [Fact]
    public void Parse_ValidFormat_ReturnsCorrectInjection()
    {
        var result = HexShapeInjection.Parse("pair@0,0");

        result.PatternName.ShouldBe("pair");
        result.Position.ShouldBe(new HexPoint(0, 0));
    }

    [Fact]
    public void Parse_PositiveCoordinates_ReturnsCorrectPosition()
    {
        var result = HexShapeInjection.Parse("triangle@5,8");

        result.PatternName.ShouldBe("triangle");
        result.Position.Q.ShouldBe(5);
        result.Position.R.ShouldBe(8);
    }

    [Fact]
    public void Parse_NegativeCoordinates_ReturnsCorrectPosition()
    {
        var result = HexShapeInjection.Parse("hex-ring@-5,-8");

        result.PatternName.ShouldBe("hex-ring");
        result.Position.Q.ShouldBe(-5);
        result.Position.R.ShouldBe(-8);
    }

    [Fact]
    public void Parse_MixedCoordinates_ReturnsCorrectPosition()
    {
        var result = HexShapeInjection.Parse("triple-triangle@-8,12");

        result.PatternName.ShouldBe("triple-triangle");
        result.Position.Q.ShouldBe(-8);
        result.Position.R.ShouldBe(12);
    }

    [Fact]
    public void Parse_PatternNameWithHyphen_ReturnsCorrectName()
    {
        var result = HexShapeInjection.Parse("my-pattern-name@1,2");

        result.PatternName.ShouldBe("my-pattern-name");
    }

    [Fact]
    public void Parse_PatternNameWithUnderscore_ReturnsCorrectName()
    {
        var result = HexShapeInjection.Parse("my_pattern@3,4");

        result.PatternName.ShouldBe("my_pattern");
    }

    [Fact]
    public void Parse_LargeCoordinates_ReturnsCorrectPosition()
    {
        var result = HexShapeInjection.Parse("pattern@1000,-999");

        result.Position.Q.ShouldBe(1000);
        result.Position.R.ShouldBe(-999);
    }

    #endregion

    #region Parse Invalid Input Tests

    [Fact]
    public void Parse_NullValue_ThrowsArgumentNullException() => _ = Should.Throw<ArgumentNullException>(() => HexShapeInjection.Parse(null!));

    [Fact]
    public void Parse_MissingAtSymbol_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("pair0,0"));

        ex.Message.ShouldContain("pair0,0");
        ex.Message.ShouldContain("name@q,r");
    }

    [Fact]
    public void Parse_EmptyPatternName_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("@0,0"));

        ex.Message.ShouldContain("Pattern name cannot be empty");
    }

    [Fact]
    public void Parse_WhitespacePatternName_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("   @0,0"));

        ex.Message.ShouldContain("Pattern name cannot be empty");
    }

    [Fact]
    public void Parse_MissingComma_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("pair@00"));

        ex.Message.ShouldContain("pair@00");
        ex.Message.ShouldContain("name@q,r");
    }

    [Fact]
    public void Parse_InvalidQCoordinate_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("pair@x,0"));

        ex.Message.ShouldContain("Invalid Q coordinate");
        ex.Message.ShouldContain("x");
    }

    [Fact]
    public void Parse_InvalidRCoordinate_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("pair@0,y"));

        ex.Message.ShouldContain("Invalid R coordinate");
        ex.Message.ShouldContain("y");
    }

    [Fact]
    public void Parse_EmptyQCoordinate_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("pair@,0"));

        ex.Message.ShouldContain("Invalid Q coordinate");
    }

    [Fact]
    public void Parse_EmptyRCoordinate_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("pair@0,"));

        ex.Message.ShouldContain("Invalid R coordinate");
    }

    [Fact]
    public void Parse_FloatingPointCoordinate_ThrowsFormatException()
    {
        FormatException ex = Should.Throw<FormatException>(() => HexShapeInjection.Parse("pair@1.5,0"));

        ex.Message.ShouldContain("Invalid Q coordinate");
    }

    #endregion

    #region Record Struct Tests

    [Fact]
    public void HexShapeInjection_Equality_WorksCorrectly()
    {
        var a = new HexShapeInjection("pair", (1, 2));
        var b = new HexShapeInjection("pair", (1, 2));
        var c = new HexShapeInjection("pair", (1, 3));

        a.ShouldBe(b);
        a.ShouldNotBe(c);
    }

    [Fact]
    public void HexShapeInjection_ToString_ContainsPatternAndPosition()
    {
        var injection = new HexShapeInjection("triangle", (-5, 8));

        string str = injection.ToString();

        str.ShouldContain("triangle");
        str.ShouldContain("-5");
        str.ShouldContain("8");
    }

    #endregion
}
