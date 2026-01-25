using GameOfLife.Core.Seeds;

namespace GameOfLife.Core.Tests;

public class SeedInjectionTests
{
    [Fact]
    public void Apply_InBoundsOffset_ShiftsAlivePoints()
    {
        var pattern = SeedPatternParser.Parse(["#."]);

        var injected = SeedInjection.Apply(pattern, new Point2D(1, 2), 4, 4);

        Assert.Contains(new Point2D(1, 2), injected);
        Assert.DoesNotContain(new Point2D(0, 0), injected);
    }

    [Fact]
    public void Apply_OutOfBoundsOffset_Throws()
    {
        var pattern = SeedPatternParser.Parse(["#."]);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            SeedInjection.Apply(pattern, new Point2D(4, 0), 4, 4));

        Assert.Equal("offset", exception.ParamName);
    }
}
