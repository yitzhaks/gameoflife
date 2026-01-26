using GameOfLife.Core;
using GameOfLife.Rendering;
using GameOfLife.Rendering.Console;

namespace GameOfLife.Console;

/// <summary>
/// Controls the game loop and user interaction.
/// </summary>
internal sealed class GameController
{
    private readonly CommandLineOptions _options;
    private readonly ShapeLoader _shapeLoader;
    private readonly TextWriter _output;
    private readonly TextReader _input;

    /// <summary>
    /// Creates a new game controller.
    /// </summary>
    /// <param name="options">The command-line options.</param>
    /// <param name="shapeLoader">The shape loader for loading patterns.</param>
    /// <param name="output">The text writer for output.</param>
    /// <param name="input">The text reader for input.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public GameController(
        CommandLineOptions options,
        ShapeLoader shapeLoader,
        TextWriter output,
        TextReader input)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(shapeLoader);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);

        _options = options;
        _shapeLoader = shapeLoader;
        _output = output;
        _input = input;
    }

    /// <summary>
    /// Runs the game loop. Returns exit code (0 = success).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exit code (0 = success).</returns>
    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Create topology
            var topology = new Grid2DTopology(_options.Width, _options.Height);

            // Create initial generation with injected shapes
            var initialStates = new Dictionary<Point2D, bool>();
            foreach (var injection in _options.Injections)
            {
                var pattern = _shapeLoader.LoadPattern(injection.PatternName);
                foreach (var point in pattern)
                {
                    var targetX = injection.X + point.X;
                    var targetY = injection.Y + point.Y;

                    // Clip points outside bounds
                    if (targetX >= 0 && targetX < _options.Width &&
                        targetY >= 0 && targetY < _options.Height)
                    {
                        initialStates[new Point2D(targetX, targetY)] = true;
                    }
                }
            }

            var initialGeneration = new DictionaryGeneration<Point2D, bool>(initialStates, false);

            // Create world and timeline
            var world = new World<Point2D, bool>(topology, new ClassicRules());
            var timeline = new Timeline<Point2D, bool>(world, initialGeneration);

            // Create renderer
            var layoutEngine = new IdentityLayoutEngine();
            var theme = ConsoleTheme.Default;
            var renderer = new ConsoleRenderer(_output, layoutEngine, theme);

            // Run game loop
            int generation = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                // Clear console (only when running interactively)
                if (ReferenceEquals(_output, System.Console.Out) && !System.Console.IsOutputRedirected)
                {
                    System.Console.Clear();
                }

                // Display generation number
                await _output.WriteLineAsync($"Generation: {generation}");
                await _output.WriteLineAsync();

                // Render current state
                renderer.Render(topology, timeline.Current);

                await _output.WriteLineAsync();

                // Display controls
                if (_options.Auto)
                {
                    await _output.WriteLineAsync("Q/Esc: Quit");
                }
                else
                {
                    await _output.WriteLineAsync("Space/Enter: Next | Q/Esc: Quit");
                }

                // Check max generations
                if (_options.MaxGenerations is { } maxGen && generation >= maxGen)
                {
                    await _output.WriteLineAsync();
                    await _output.WriteLineAsync($"Reached maximum generations ({maxGen}).");
                    break;
                }

                // Handle input
                var isInteractiveConsole = ReferenceEquals(_input, System.Console.In) &&
                                           !System.Console.IsInputRedirected;

                if (_options.Auto)
                {
                    // In auto mode: wait for delay, check for Q/Esc key
                    var waitStart = DateTime.UtcNow;
                    while ((DateTime.UtcNow - waitStart).TotalMilliseconds < _options.Delay)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return 0;
                        }

                        if (isInteractiveConsole && System.Console.KeyAvailable)
                        {
                            var key = System.Console.ReadKey(true);
                            if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                            {
                                return 0;
                            }
                        }

                        await Task.Delay(10, cancellationToken);
                    }
                }
                else
                {
                    // In interactive mode: wait for keypress
                    if (isInteractiveConsole)
                    {
                        while (!System.Console.KeyAvailable)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return 0;
                            }

                            await Task.Delay(10, cancellationToken);
                        }

                        var key = System.Console.ReadKey(true);
                        if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                        {
                            return 0;
                        }

                        // Space or Enter advances to next generation
                        if (key.Key is not ConsoleKey.Spacebar and not ConsoleKey.Enter)
                        {
                            continue; // Ignore other keys
                        }
                    }
                    else
                    {
                        // When input is redirected, read a line
                        var line = await _input.ReadLineAsync(cancellationToken);
                        if (line is null || line.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
                        {
                            return 0;
                        }
                    }
                }

                // Advance to next generation
                timeline.Step();
                generation++;
            }

            return 0;
        }
        catch (FileNotFoundException ex)
        {
            await _output.WriteLineAsync($"Error: {ex.Message}");
            return 1;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
    }
}
