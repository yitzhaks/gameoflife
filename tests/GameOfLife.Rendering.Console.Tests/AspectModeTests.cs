using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class AspectModeTests
{
    [Fact]
    public void AspectMode_None_HasValue0() => ((int)AspectMode.None).ShouldBe(0);

    [Fact]
    public void AspectMode_HalfBlock_HasValue1() => ((int)AspectMode.HalfBlock).ShouldBe(1);

    [Fact]
    public void AspectMode_DefaultValue_IsNone()
    {
        var mode = default(AspectMode);

        mode.ShouldBe(AspectMode.None);
    }

    [Theory]
    [InlineData(AspectMode.None)]
    [InlineData(AspectMode.HalfBlock)]
    public void AspectMode_ToString_ReturnsName(AspectMode mode)
    {
        string name = mode.ToString();

        name.ShouldBe(mode switch
        {
            AspectMode.None => "None",
            AspectMode.HalfBlock => "HalfBlock",
            _ => throw new ArgumentException("Unknown aspect mode", nameof(mode))
        });
    }
}
