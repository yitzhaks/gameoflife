using System.CommandLine;

namespace GameOfLife.Console;

/// <summary>
/// Parses command-line arguments into CommandLineOptions.
/// </summary>
internal static class CommandLineParser
{
    /// <summary>
    /// Creates the root command with all options configured.
    /// </summary>
    /// <param name="handler">The handler to invoke with the parsed options.</param>
    /// <returns>The configured root command.</returns>
    /// <exception cref="ArgumentNullException">Thrown if handler is null.</exception>
    public static RootCommand CreateRootCommand(Func<CommandLineOptions, Task<int>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var widthOption = new Option<int>(name: "--width", aliases: ["-w"])
        {
            Description = "The width of the grid.",
            DefaultValueFactory = _ => 20
        };

        var heightOption = new Option<int>(name: "--height")
        {
            Description = "The height of the grid.",
            DefaultValueFactory = _ => 20
        };

        var injectOption = new Option<string[]>(name: "--inject", aliases: ["-i"])
        {
            Description = "Inject a shape at a position (format: name@x,y). Can be specified multiple times.",
            AllowMultipleArgumentsPerToken = true
        };

        var generationsOption = new Option<int?>(name: "--generations", aliases: ["-g"])
        {
            Description = "Maximum number of generations to run (default: unlimited).",
            DefaultValueFactory = _ => null
        };

        var startAutoplayOption = new Option<bool>(name: "--start-autoplay", aliases: ["-a"])
        {
            Description = "Start in autoplay mode.",
            DefaultValueFactory = _ => false
        };

        var maxFpsOption = new Option<int>(name: "--max-fps")
        {
            Description = "Maximum frames per second during autoplay (default: 30).",
            DefaultValueFactory = _ => 30
        };

        var rootCommand = new RootCommand("Conway's Game of Life console application")
        {
            widthOption,
            heightOption,
            injectOption,
            generationsOption,
            startAutoplayOption,
            maxFpsOption
        };

        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var width = parseResult.GetValue(widthOption);
            var height = parseResult.GetValue(heightOption);
            var injections = parseResult.GetValue(injectOption);
            var generations = parseResult.GetValue(generationsOption);
            var startAutoplay = parseResult.GetValue(startAutoplayOption);
            var maxFps = parseResult.GetValue(maxFpsOption);

            var options = new CommandLineOptions
            {
                Width = width,
                Height = height,
                MaxGenerations = generations,
                StartAutoplay = startAutoplay,
                MaxFps = maxFps
            };

            // Parse shape injections
            if (injections is not null)
            {
                foreach (var injection in injections)
                {
                    options.Injections.Add(ShapeInjection.Parse(injection));
                }
            }

            Environment.ExitCode = await handler(options);
        });

        return rootCommand;
    }
}
