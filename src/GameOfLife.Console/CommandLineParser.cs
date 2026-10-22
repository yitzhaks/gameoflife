using System.CommandLine;

using GameOfLife.Rendering.Console;

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
            Description = "The width of the grid (for rectangular topology).",
            DefaultValueFactory = _ => 20
        };

        var heightOption = new Option<int>(name: "--height")
        {
            Description = "The height of the grid (for rectangular topology).",
            DefaultValueFactory = _ => 20
        };

        Option<string> topologyOption = new Option<string>(name: "--topology")
        {
            Description = "Board topology: rect (rectangular) or hex (hexagonal).",
            DefaultValueFactory = _ => "rect"
        }.AcceptOnlyFromAmong("rect", "hex");

        var hexRadiusOption = new Option<int>(name: "--hex-radius")
        {
            Description = "Radius for hexagonal topology (default: 10).",
            DefaultValueFactory = _ => 10
        };

        var hexFillOption = new Option<int>(name: "--hex-fill")
        {
            Description = "Percentage of cells to randomly fill for hex boards (0-100).",
            DefaultValueFactory = _ => 0
        };

        Option<string> hexRulesOption = new Option<string>(name: "--hex-rules")
        {
            Description = "Hex rule set: B2S34 (default), B2S35, B24S35, B2S23, or classic (B3S23).",
            DefaultValueFactory = _ => "B2S34"
        }.AcceptOnlyFromAmong("B2S34", "B2S35", "B24S35", "B2S23", "classic");

        var analyzeOption = new Option<bool>(name: "--analyze")
        {
            Description = "Run hex pattern analysis to find oscillators and still lifes.",
            DefaultValueFactory = _ => false
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

        Option<string> aspectModeOption = new Option<string>(name: "--aspect-mode")
        {
            Description = "Aspect ratio correction mode: none (1 char per cell) or half-block (2 cells per char). Only for rectangular topology.",
            DefaultValueFactory = _ => "none"
        }.AcceptOnlyFromAmong("none", "half-block");

        var hexInjectOption = new Option<string[]>(name: "--hex-inject")
        {
            Description = "Inject a hex shape at a position (format: name@q,r). Can be specified multiple times.",
            AllowMultipleArgumentsPerToken = true
        };

        var rootCommand = new RootCommand("Conway's Game of Life console application")
        {
            widthOption,
            heightOption,
            topologyOption,
            hexRadiusOption,
            hexFillOption,
            hexRulesOption,
            analyzeOption,
            injectOption,
            generationsOption,
            startAutoplayOption,
            maxFpsOption,
            aspectModeOption,
            hexInjectOption
        };

        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            int width = parseResult.GetValue(widthOption);
            int height = parseResult.GetValue(heightOption);
            string topologyString = parseResult.GetValue(topologyOption) ?? "rect";
            int hexRadius = parseResult.GetValue(hexRadiusOption);
            int hexFillPercent = parseResult.GetValue(hexFillOption);
            string hexRules = parseResult.GetValue(hexRulesOption) ?? "B2S34";
            bool analyzePatterns = parseResult.GetValue(analyzeOption);
            string[]? injections = parseResult.GetValue(injectOption);
            int? generations = parseResult.GetValue(generationsOption);
            bool startAutoplay = parseResult.GetValue(startAutoplayOption);
            int maxFps = parseResult.GetValue(maxFpsOption);
            string aspectModeString = parseResult.GetValue(aspectModeOption) ?? "none";
            string[]? hexInjections = parseResult.GetValue(hexInjectOption);

            BoardTopology topology = topologyString == "hex" ? BoardTopology.Hexagonal : BoardTopology.Rectangular;
            AspectMode aspectMode = aspectModeString == "half-block" ? AspectMode.HalfBlock : AspectMode.None;

            var options = new CommandLineOptions
            {
                Width = width,
                Height = height,
                Topology = topology,
                HexRadius = hexRadius,
                HexFillPercent = Math.Clamp(hexFillPercent, 0, 100),
                HexRules = hexRules,
                AnalyzePatterns = analyzePatterns,
                MaxGenerations = generations,
                StartAutoplay = startAutoplay,
                MaxFps = maxFps,
                AspectMode = aspectMode
            };

            // Parse shape injections
            if (injections is not null)
            {
                foreach (string? injection in injections)
                {
                    options.Injections.Add(ShapeInjection.Parse(injection));
                }
            }

            // Parse hex shape injections
            if (hexInjections is not null)
            {
                foreach (string? injection in hexInjections)
                {
                    options.HexInjections.Add(HexShapeInjection.Parse(injection));
                }
            }

            Environment.ExitCode = await handler(options);
        });

        return rootCommand;
    }
}
