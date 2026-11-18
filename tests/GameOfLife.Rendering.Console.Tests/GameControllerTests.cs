using GameOfLife.Console;
using GameOfLife.Core;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class GameControllerTests
{
    private static ShapeLoader CreateShapeLoader(string path = ".") => new(path);

    private static HexShapeLoader CreateHexShapeLoader(string path = ".") => new(path);

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        _ = Should.Throw<ArgumentNullException>(() => new GameController(null!, loader, hexLoader, output, input));
    }

    [Fact]
    public void Constructor_NullShapeLoader_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        var options = new CommandLineOptions();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        _ = Should.Throw<ArgumentNullException>(() => new GameController(options, null!, hexLoader, output, input));
    }

    [Fact]
    public void Constructor_NullHexShapeLoader_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        using var input = new StringReader("");
        var options = new CommandLineOptions();
        ShapeLoader loader = CreateShapeLoader();

        _ = Should.Throw<ArgumentNullException>(() => new GameController(options, loader, null!, output, input));
    }

    [Fact]
    public void Constructor_NullOutput_ThrowsArgumentNullException()
    {
        using var input = new StringReader("");
        var options = new CommandLineOptions();
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        _ = Should.Throw<ArgumentNullException>(() => new GameController(options, loader, hexLoader, null!, input));
    }

    [Fact]
    public void Constructor_NullInput_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var options = new CommandLineOptions();
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        _ = Should.Throw<ArgumentNullException>(() => new GameController(options, loader, hexLoader, output, null!));
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
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        var controller = new GameController(options, loader, hexLoader, output, input);
        int result = await controller.RunAsync();

        result.ShouldBe(0);
        output.ToString().ShouldContain("Generation: 0");
        output.ToString().ShouldContain("Reached maximum generations (0)");
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
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        var controller = new GameController(options, loader, hexLoader, output, input);
        int result = await controller.RunAsync();

        result.ShouldBe(0);
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
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        var controller = new GameController(options, loader, hexLoader, output, input);
        int result = await controller.RunAsync();

        result.ShouldBe(0);
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
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        var controller = new GameController(options, loader, hexLoader, output, input);
        int result = await controller.RunAsync();

        string outputText = output.ToString();
        result.ShouldBe(0);
        outputText.ShouldContain("Generation: 0");
        outputText.ShouldContain("Generation: 1");
        outputText.ShouldContain("Generation: 2");
        outputText.ShouldContain("Generation: 3");
    }

    [Fact]
    public async Task RunAsync_WithInjection_LoadsPattern()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
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
                Injections = [new ShapeInjection("block", (2, 2))]
            };
            var loader = new ShapeLoader(tempDir);
            HexShapeLoader hexLoader = CreateHexShapeLoader();

            var controller = new GameController(options, loader, hexLoader, output, input);
            int result = await controller.RunAsync();

            result.ShouldBe(0);
            // The block should be rendered somewhere in the output
            string outputText = output.ToString();
            outputText.ShouldContain("Generation: 0");
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
            Injections = [new ShapeInjection("nonexistent_pattern_12345", default)]
        };
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        var controller = new GameController(options, loader, hexLoader, output, input);
        int result = await controller.RunAsync();

        result.ShouldBe(1);
        output.ToString().ShouldContain("Error:");
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
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        var controller = new GameController(options, loader, hexLoader, output, input);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        int result = await controller.RunAsync(cts.Token);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task RunAsync_PatternClippedAtBounds_DoesNotThrow()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
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
                Injections = [new ShapeInjection("wide", default)]
            };
            var loader = new ShapeLoader(tempDir);
            HexShapeLoader hexLoader = CreateHexShapeLoader();

            var controller = new GameController(options, loader, hexLoader, output, input);
            int result = await controller.RunAsync();

            result.ShouldBe(0);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task RunAsync_PatternAtNegativeOffset_ClipsCorrectly()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "block.txt"), "##\n##");

            using var output = new StringWriter();
            using var input = new StringReader("q\n");
            var options = new CommandLineOptions
            {
                Width = 5,
                Height = 5,
                Injections = [new ShapeInjection("block", (-1, -1))]
            };
            var loader = new ShapeLoader(tempDir);
            HexShapeLoader hexLoader = CreateHexShapeLoader();

            var controller = new GameController(options, loader, hexLoader, output, input);
            int result = await controller.RunAsync();

            result.ShouldBe(0);
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
        ShapeLoader loader = CreateShapeLoader();
        HexShapeLoader hexLoader = CreateHexShapeLoader();

        var controller = new GameController(options, loader, hexLoader, output, input);
        _ = await controller.RunAsync();

        output.ToString().ShouldContain("Space/Enter: Next | P: Play | Q/Esc: Quit");
    }

    [Fact]
    public async Task RunAsync_MultipleInjections_LoadsAllPatterns()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
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
                    new ShapeInjection("block", (2, 2)),
                    new ShapeInjection("blinker", (10, 10))
                ]
            };
            var loader = new ShapeLoader(tempDir);
            HexShapeLoader hexLoader = CreateHexShapeLoader();

            var controller = new GameController(options, loader, hexLoader, output, input);
            int result = await controller.RunAsync();

            result.ShouldBe(0);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
