using GameOfLife.Console;
using GameOfLife.Core;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class CommandLineOptionsTests
{
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        var options = new CommandLineOptions();

        options.Width.ShouldBe(20);
        options.Height.ShouldBe(20);
        options.MaxGenerations.ShouldBeNull();
        options.StartAutoplay.ShouldBeFalse();
        options.MaxFps.ShouldBe(30);
        options.Injections.ShouldBeEmpty();
    }

    [Fact]
    public void Width_CanBeSet()
    {
        var options = new CommandLineOptions { Width = 50 };

        options.Width.ShouldBe(50);
    }

    [Fact]
    public void Height_CanBeSet()
    {
        var options = new CommandLineOptions { Height = 40 };

        options.Height.ShouldBe(40);
    }

    [Fact]
    public void MaxGenerations_CanBeSet()
    {
        var options = new CommandLineOptions { MaxGenerations = 100 };

        options.MaxGenerations.ShouldBe(100);
    }

    [Fact]
    public void StartAutoplay_CanBeSet()
    {
        var options = new CommandLineOptions { StartAutoplay = true };

        options.StartAutoplay.ShouldBeTrue();
    }

    [Fact]
    public void MaxFps_CanBeSet()
    {
        var options = new CommandLineOptions { MaxFps = 60 };

        options.MaxFps.ShouldBe(60);
    }

    [Fact]
    public void Injections_CanBeSet()
    {
        var injections = new List<ShapeInjection>
        {
            new("glider", (5, 10)),
            new("block", default)
        };
        var options = new CommandLineOptions { Injections = injections };

        options.Injections.Count.ShouldBe(2);
        options.Injections[0].PatternName.ShouldBe("glider");
        options.Injections[1].PatternName.ShouldBe("block");
    }
}
