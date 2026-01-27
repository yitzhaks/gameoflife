using System.CommandLine;

using GameOfLife.Console;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void CreateRootCommand_NullHandler_ThrowsArgumentNullException() => Should.Throw<ArgumentNullException>(() => CommandLineParser.CreateRootCommand(null!));

    [Fact]
    public void CreateRootCommand_ReturnsRootCommand()
    {
        RootCommand command = CommandLineParser.CreateRootCommand(_ => Task.FromResult(0));

        _ = command.ShouldNotBeNull();
        command.Description.ShouldBe("Conway's Game of Life console application");
    }

    [Fact]
    public void CreateRootCommand_NoArguments_UsesDefaultValues()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse([]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.Width.ShouldBe(20);
        capturedOptions.Height.ShouldBe(20);
        capturedOptions.MaxGenerations.ShouldBeNull();
        capturedOptions.StartAutoplay.ShouldBeFalse();
        capturedOptions.MaxFps.ShouldBe(30);
        capturedOptions.Injections.ShouldBeEmpty();
    }

    [Fact]
    public void CreateRootCommand_WidthOption_SetsWidth()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["--width", "50"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.Width.ShouldBe(50);
    }

    [Fact]
    public void CreateRootCommand_WidthShortOption_SetsWidth()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["-w", "30"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.Width.ShouldBe(30);
    }

    [Fact]
    public void CreateRootCommand_HeightOption_SetsHeight()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["--height", "40"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.Height.ShouldBe(40);
    }

    [Fact]
    public void CreateRootCommand_GenerationsOption_SetsMaxGenerations()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["--generations", "100"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.MaxGenerations.ShouldBe(100);
    }

    [Fact]
    public void CreateRootCommand_GenerationsShortOption_SetsMaxGenerations()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["-g", "50"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.MaxGenerations.ShouldBe(50);
    }

    [Fact]
    public void CreateRootCommand_StartAutoplayOption_SetsStartAutoplay()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["--start-autoplay"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.StartAutoplay.ShouldBeTrue();
    }

    [Fact]
    public void CreateRootCommand_StartAutoplayShortOption_SetsStartAutoplay()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["-a"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.StartAutoplay.ShouldBeTrue();
    }

    [Fact]
    public void CreateRootCommand_MaxFpsOption_SetsMaxFps()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["--max-fps", "60"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.MaxFps.ShouldBe(60);
    }

    [Fact]
    public void CreateRootCommand_InjectOption_AddsInjection()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["--inject", "glider@5,10"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        _ = capturedOptions.Injections.ShouldHaveSingleItem();
        capturedOptions.Injections[0].PatternName.ShouldBe("glider");
        capturedOptions.Injections[0].Position.X.ShouldBe(5);
        capturedOptions.Injections[0].Position.Y.ShouldBe(10);
    }

    [Fact]
    public void CreateRootCommand_InjectShortOption_AddsInjection()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["-i", "block@0,0"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        _ = capturedOptions.Injections.ShouldHaveSingleItem();
        capturedOptions.Injections[0].PatternName.ShouldBe("block");
    }

    [Fact]
    public void CreateRootCommand_MultipleInjectOptions_AddsAllInjections()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse(["-i", "glider@5,10", "-i", "block@0,0", "-i", "blinker@15,15"]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.Injections.Count.ShouldBe(3);
    }

    [Fact]
    public void CreateRootCommand_AllOptions_SetsAllValues()
    {
        CommandLineOptions? capturedOptions = null;
        RootCommand command = CommandLineParser.CreateRootCommand(options =>
        {
            capturedOptions = options;
            return Task.FromResult(0);
        });

        _ = command.Parse([
            "--width", "80",
            "--height", "60",
            "--generations", "1000",
            "--inject", "glider@10,10"
        ]).Invoke();

        _ = capturedOptions.ShouldNotBeNull();
        capturedOptions.Width.ShouldBe(80);
        capturedOptions.Height.ShouldBe(60);
        capturedOptions.MaxGenerations.ShouldBe(1000);
        _ = capturedOptions.Injections.ShouldHaveSingleItem();
    }

    [Fact]
    public void CreateRootCommand_HandlerReturnsExitCode_SetsEnvironmentExitCode()
    {
        RootCommand command = CommandLineParser.CreateRootCommand(_ => Task.FromResult(42));

        _ = command.Parse([]).Invoke();

        Environment.ExitCode.ShouldBe(42);

        // Reset for other tests
        Environment.ExitCode = 0;
    }
}
