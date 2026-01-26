using GameOfLife.Console;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class CommandLineOptionsTests
{
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var options = new CommandLineOptions();

        Assert.Equal(20, options.Width);
        Assert.Equal(20, options.Height);
        Assert.Equal(200, options.Delay);
        Assert.False(options.Auto);
        Assert.Null(options.MaxGenerations);
        Assert.Empty(options.Injections);
    }

    [Fact]
    public void Width_CanBeSet()
    {
        var options = new CommandLineOptions { Width = 50 };

        Assert.Equal(50, options.Width);
    }

    [Fact]
    public void Height_CanBeSet()
    {
        var options = new CommandLineOptions { Height = 40 };

        Assert.Equal(40, options.Height);
    }

    [Fact]
    public void Delay_CanBeSet()
    {
        var options = new CommandLineOptions { Delay = 500 };

        Assert.Equal(500, options.Delay);
    }

    [Fact]
    public void Auto_CanBeSet()
    {
        var options = new CommandLineOptions { Auto = true };

        Assert.True(options.Auto);
    }

    [Fact]
    public void MaxGenerations_CanBeSet()
    {
        var options = new CommandLineOptions { MaxGenerations = 100 };

        Assert.Equal(100, options.MaxGenerations);
    }

    [Fact]
    public void Injections_CanBeSet()
    {
        var injections = new List<ShapeInjection>
        {
            new("glider", 5, 10),
            new("block", 0, 0)
        };
        var options = new CommandLineOptions { Injections = injections };

        Assert.Equal(2, options.Injections.Count);
        Assert.Equal("glider", options.Injections[0].PatternName);
        Assert.Equal("block", options.Injections[1].PatternName);
    }
}
