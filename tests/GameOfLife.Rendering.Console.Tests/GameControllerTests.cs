using GameOfLife.Console;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class GameControllerTests
{
    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        var loader = new ShapeLoader(".");

        Assert.Throws<ArgumentNullException>(() => new GameController(null!, loader, output, input));
    }

    [Fact]
    public void Constructor_NullShapeLoader_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        var options = new CommandLineOptions();

        Assert.Throws<ArgumentNullException>(() => new GameController(options, null!, output, input));
    }

    [Fact]
    public void Constructor_NullOutput_ThrowsArgumentNullException()
    {
        using var input = new StringReader("");
        var options = new CommandLineOptions();
        var loader = new ShapeLoader(".");

        Assert.Throws<ArgumentNullException>(() => new GameController(options, loader, null!, input));
    }

    [Fact]
    public void Constructor_NullInput_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var options = new CommandLineOptions();
        var loader = new ShapeLoader(".");

        Assert.Throws<ArgumentNullException>(() => new GameController(options, loader, output, null!));
    }

    [Fact]
    public async Task RunAsync_MaxGenerationsZero_ExitsImmediately()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        var options = new CommandLineOptions
        {
            Width = 5,
            Height = 5,
            MaxGenerations = 0
        };
        var loader = new ShapeLoader(".");

        var controller = new GameController(options, loader, output, input);
        var result = await controller.RunAsync();

        Assert.Equal(0, result);
        Assert.Contains("Generation: 0", output.ToString());
        Assert.Contains("Reached maximum generations (0)", output.ToString());
    }

    [Fact]
    public async Task RunAsync_NonInteractiveWithQuit_ExitsGracefully()
    {
        using var output = new StringWriter();
        using var input = new StringReader("q\n");
        var options = new CommandLineOptions
        {
            Width = 5,
            Height = 5
        };
        var loader = new ShapeLoader(".");

        var controller = new GameController(options, loader, output, input);
        var result = await controller.RunAsync();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_NonInteractiveWithEmptyInput_ExitsGracefully()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        var options = new CommandLineOptions
        {
            Width = 5,
            Height = 5
        };
        var loader = new ShapeLoader(".");

        var controller = new GameController(options, loader, output, input);
        var result = await controller.RunAsync();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_NonInteractiveAdvanceGenerations_AdvancesCorrectly()
    {
        using var output = new StringWriter();
        // Three newlines to advance three generations, then quit
        using var input = new StringReader("\n\n\nq\n");
        var options = new CommandLineOptions
        {
            Width = 5,
            Height = 5
        };
        var loader = new ShapeLoader(".");

        var controller = new GameController(options, loader, output, input);
        var result = await controller.RunAsync();

        var outputText = output.ToString();
        Assert.Equal(0, result);
        Assert.Contains("Generation: 0", outputText);
        Assert.Contains("Generation: 1", outputText);
        Assert.Contains("Generation: 2", outputText);
        Assert.Contains("Generation: 3", outputText);
    }

    [Fact]
    public async Task RunAsync_WithInjection_LoadsPattern()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create a simple block pattern
            await File.WriteAllTextAsync(Path.Combine(tempDir, "block.txt"), "##\n##");

            using var output = new StringWriter();
            using var input = new StringReader("q\n");
            var options = new CommandLineOptions
            {
                Width = 10,
                Height = 10,
                Injections = [new ShapeInjection("block", 2, 2)]
            };
            var loader = new ShapeLoader(tempDir);

            var controller = new GameController(options, loader, output, input);
            var result = await controller.RunAsync();

            Assert.Equal(0, result);
            // The block should be rendered somewhere in the output
            var outputText = output.ToString();
            Assert.Contains("Generation: 0", outputText);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task RunAsync_WithNonExistentPattern_ReturnsErrorCode()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        var options = new CommandLineOptions
        {
            Width = 10,
            Height = 10,
            Injections = [new ShapeInjection("nonexistent_pattern_12345", 0, 0)]
        };
        var loader = new ShapeLoader(".");

        var controller = new GameController(options, loader, output, input);
        var result = await controller.RunAsync();

        Assert.Equal(1, result);
        Assert.Contains("Error:", output.ToString());
    }

    [Fact]
    public async Task RunAsync_WithCancellation_ExitsGracefully()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        var options = new CommandLineOptions
        {
            Width = 5,
            Height = 5,
            MaxGenerations = 1000
        };
        var loader = new ShapeLoader(".");

        var controller = new GameController(options, loader, output, input);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        var result = await controller.RunAsync(cts.Token);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task RunAsync_PatternClippedAtBounds_DoesNotThrow()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create a pattern that extends beyond bounds
            await File.WriteAllTextAsync(Path.Combine(tempDir, "wide.txt"), "#####");

            using var output = new StringWriter();
            using var input = new StringReader("q\n");
            var options = new CommandLineOptions
            {
                Width = 3,
                Height = 3,
                Injections = [new ShapeInjection("wide", 0, 0)]
            };
            var loader = new ShapeLoader(tempDir);

            var controller = new GameController(options, loader, output, input);
            var result = await controller.RunAsync();

            Assert.Equal(0, result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task RunAsync_PatternAtNegativeOffset_ClipsCorrectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "block.txt"), "##\n##");

            using var output = new StringWriter();
            using var input = new StringReader("q\n");
            var options = new CommandLineOptions
            {
                Width = 5,
                Height = 5,
                Injections = [new ShapeInjection("block", -1, -1)]
            };
            var loader = new ShapeLoader(tempDir);

            var controller = new GameController(options, loader, output, input);
            var result = await controller.RunAsync();

            Assert.Equal(0, result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task RunAsync_StepMode_DisplaysStepModeControls()
    {
        using var output = new StringWriter();
        using var input = new StringReader("q\n");
        var options = new CommandLineOptions
        {
            Width = 5,
            Height = 5
        };
        var loader = new ShapeLoader(".");

        var controller = new GameController(options, loader, output, input);
        await controller.RunAsync();

        Assert.Contains("Space/Enter: Next | P: Play | Q/Esc: Quit", output.ToString());
    }

    [Fact]
    public async Task RunAsync_MultipleInjections_LoadsAllPatterns()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "block.txt"), "##\n##");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "blinker.txt"), "###");

            using var output = new StringWriter();
            using var input = new StringReader("q\n");
            var options = new CommandLineOptions
            {
                Width = 20,
                Height = 20,
                Injections =
                [
                    new ShapeInjection("block", 2, 2),
                    new ShapeInjection("blinker", 10, 10)
                ]
            };
            var loader = new ShapeLoader(tempDir);

            var controller = new GameController(options, loader, output, input);
            var result = await controller.RunAsync();

            Assert.Equal(0, result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
