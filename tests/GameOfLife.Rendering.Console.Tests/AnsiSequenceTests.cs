using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class AnsiSequenceTests
{
    [Theory]
    [InlineData(AnsiSequence.Reset, "\x1b[0m")]
    [InlineData(AnsiSequence.ForegroundGreen, "\x1b[32m")]
    [InlineData(AnsiSequence.ForegroundDarkGray, "\x1b[90m")]
    [InlineData(AnsiSequence.ForegroundGray, "\x1b[37m")]
    public void ToAnsiString_KnownSequence_ReturnsCorrectString(AnsiSequence sequence, string expected)
    {
        string result = sequence.ToAnsiString();

        result.ShouldBe(expected);
    }

    [Fact]
    public void ToAnsiString_UnknownSequence_ThrowsArgumentOutOfRangeException()
    {
        var unknownSequence = (AnsiSequence)255;

        _ = Should.Throw<ArgumentOutOfRangeException>(() => unknownSequence.ToAnsiString());
    }
}
