using GameOfLife.Core.Seeds;

namespace GameOfLife.Core.Tests;

public class SeedPatternTests
{
    [Fact]
    public void Parse_ValidPattern_CapturesAlivePoints()
    {
        var pattern = SeedPatternParser.Parse([".#", "##"]);

        Assert.Equal(2, pattern.Width);
        Assert.Equal(2, pattern.Height);
        Assert.Contains(new Point2D(1, 0), pattern.AlivePoints);
        Assert.Contains(new Point2D(0, 1), pattern.AlivePoints);
        Assert.Contains(new Point2D(1, 1), pattern.AlivePoints);
        Assert.DoesNotContain(new Point2D(0, 0), pattern.AlivePoints);
    }

    [Fact]
    public void Parse_EmptyInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => SeedPatternParser.Parse(Array.Empty<string>()));
    }

    [Fact]
    public void Parse_InvalidCharacters_Throws()
    {
        Assert.Throws<ArgumentException>(() => SeedPatternParser.Parse([".x"]));
    }

    [Fact]
    public void Parse_InconsistentRowLengths_Throws()
    {
        Assert.Throws<ArgumentException>(() => SeedPatternParser.Parse(["#.", "#"]));
    }
}
