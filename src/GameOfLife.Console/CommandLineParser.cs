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

        var delayOption = new Option<int>(name: "--delay", aliases: ["-d"])
        {
            Description = "Delay between generations in milliseconds (auto mode).",
            DefaultValueFactory = _ => 200
        };

        var autoOption = new Option<bool>(name: "--auto", aliases: ["-a"])
        {
            Description = "Run in automatic mode (advance generations automatically).",
            DefaultValueFactory = _ => false
        };

        var generationsOption = new Option<int?>(name: "--generations", aliases: ["-g"])
        {
            Description = "Maximum number of generations to run (default: unlimited).",
            DefaultValueFactory = _ => null
        };

        var rootCommand = new RootCommand("Conway's Game of Life console application")
        {
            widthOption,
            heightOption,
            injectOption,
            delayOption,
            autoOption,
            generationsOption
        };

        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var width = parseResult.GetValue(widthOption);
            var height = parseResult.GetValue(heightOption);
            var injections = parseResult.GetValue(injectOption);
            var delay = parseResult.GetValue(delayOption);
            var auto = parseResult.GetValue(autoOption);
            var generations = parseResult.GetValue(generationsOption);

            var options = new CommandLineOptions
            {
                Width = width,
                Height = height,
                Delay = delay,
                Auto = auto,
                MaxGenerations = generations
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
