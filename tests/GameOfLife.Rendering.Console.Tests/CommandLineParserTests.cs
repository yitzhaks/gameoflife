using System.CommandLine;

using GameOfLife.Console;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void CreateRootCommand_NullHandler_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CommandLineParser.CreateRootCommand(null!));
    }

    [Fact]
    public void CreateRootCommand_ReturnsRootCommand()
    {
        var command = CommandLineParser.CreateRootCommand(_ => Task.FromResult(0));

        Assert.NotNull(command);
        Assert.Equal("Conway's Game of Life console application", command.Description);
    }

    [Fact]
    public void CreateRootCommand_NoArguments_UsesDefaultValues()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse([]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(20, capturedOptions.Width);
        Assert.Equal(20, capturedOptions.Height);
        Assert.Null(capturedOptions.MaxGenerations);
        Assert.False(capturedOptions.StartAutoplay);
        Assert.Equal(30, capturedOptions.MaxFps);
        Assert.Empty(capturedOptions.Injections);
    }

    [Fact]
    public void CreateRootCommand_WidthOption_SetsWidth()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["--width", "50"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(50, capturedOptions.Width);
    }

    [Fact]
    public void CreateRootCommand_WidthShortOption_SetsWidth()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["-w", "30"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(30, capturedOptions.Width);
    }

    [Fact]
    public void CreateRootCommand_HeightOption_SetsHeight()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["--height", "40"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(40, capturedOptions.Height);
    }

    [Fact]
    public void CreateRootCommand_GenerationsOption_SetsMaxGenerations()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["--generations", "100"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(100, capturedOptions.MaxGenerations);
    }

    [Fact]
    public void CreateRootCommand_GenerationsShortOption_SetsMaxGenerations()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["-g", "50"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(50, capturedOptions.MaxGenerations);
    }

    [Fact]
    public void CreateRootCommand_StartAutoplayOption_SetsStartAutoplay()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["--start-autoplay"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.True(capturedOptions.StartAutoplay);
    }

    [Fact]
    public void CreateRootCommand_StartAutoplayShortOption_SetsStartAutoplay()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["-a"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.True(capturedOptions.StartAutoplay);
    }

    [Fact]
    public void CreateRootCommand_MaxFpsOption_SetsMaxFps()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["--max-fps", "60"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(60, capturedOptions.MaxFps);
    }

    [Fact]
    public void CreateRootCommand_InjectOption_AddsInjection()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["--inject", "glider@5,10"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Single(capturedOptions.Injections);
        Assert.Equal("glider", capturedOptions.Injections[0].PatternName);
        Assert.Equal(5, capturedOptions.Injections[0].X);
        Assert.Equal(10, capturedOptions.Injections[0].Y);
    }

    [Fact]
    public void CreateRootCommand_InjectShortOption_AddsInjection()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["-i", "block@0,0"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Single(capturedOptions.Injections);
        Assert.Equal("block", capturedOptions.Injections[0].PatternName);
    }

    [Fact]
    public void CreateRootCommand_MultipleInjectOptions_AddsAllInjections()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse(["-i", "glider@5,10", "-i", "block@0,0", "-i", "blinker@15,15"]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(3, capturedOptions.Injections.Count);
    }

    [Fact]
    public void CreateRootCommand_AllOptions_SetsAllValues()
    {
        CommandLineOptions? capturedOptions = null;
        var command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        command.Parse([
            "--width", "80",
            "--height", "60",
            "--generations", "1000",
            "--inject", "glider@10,10"
        ]).Invoke();

        Assert.NotNull(capturedOptions);
        Assert.Equal(80, capturedOptions.Width);
        Assert.Equal(60, capturedOptions.Height);
        Assert.Equal(1000, capturedOptions.MaxGenerations);
        Assert.Single(capturedOptions.Injections);
    }

    [Fact]
    public void CreateRootCommand_HandlerReturnsExitCode_SetsEnvironmentExitCode()
    {
        var command = CommandLineParser.CreateRootCommand(_ => Task.FromResult(42));

        command.Parse([]).Invoke();

        Assert.Equal(42, Environment.ExitCode);

        // Reset for other tests
        Environment.ExitCode = 0;
    }
}
