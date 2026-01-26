using System.Diagnostics;

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

    // ANSI escape sequences for terminal control
    private const string EnterAlternateBuffer = "\x1b[?1049h";
    private const string ExitAlternateBuffer = "\x1b[?1049l";
    private const string HideCursor = "\x1b[?25l";
    private const string ShowCursor = "\x1b[?25h";
    private const string ClearScreen = "\x1b[2J";
    private const string CursorHome = "\x1b[H";

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
        var isInteractiveConsole = ReferenceEquals(_output, System.Console.Out) &&
                                   !System.Console.IsOutputRedirected;

        try
        {
            // Enter alternate screen buffer and hide cursor for interactive mode
            if (isInteractiveConsole)
            {
                await _output.WriteAsync(EnterAlternateBuffer);
                await _output.WriteAsync(HideCursor);
                await _output.WriteAsync(ClearScreen);
                await _output.WriteAsync(CursorHome);
            }

            return await RunGameLoopAsync(isInteractiveConsole, cancellationToken);
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
        finally
        {
            // Always restore terminal state
            if (isInteractiveConsole)
            {
                await _output.WriteAsync(ShowCursor);
                await _output.WriteAsync(ExitAlternateBuffer);
                await _output.FlushAsync(CancellationToken.None);
            }
        }
    }

    private async Task<int> RunGameLoopAsync(bool isInteractiveConsole, CancellationToken cancellationToken)
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
        IGeneration<Point2D, bool>? previousGeneration = null;

        // Header takes 2 lines (Generation: X + blank line), so board starts at row 3
        const int BoardStartRow = 3;

        // FPS tracking for play mode
        var fpsStopwatch = Stopwatch.StartNew();
        var frameCount = 0;
        var currentFps = 0.0;

        // Wall clock for elapsed time display
        var wallClock = Stopwatch.StartNew();

        // Play/pause state (starts based on command-line option)
        var isPlaying = _options.StartAutoplay;

        // Frame timing for FPS cap
        var frameStopwatch = new Stopwatch();

        while (!cancellationToken.IsCancellationRequested)
        {
            // Start frame timing
            frameStopwatch.Restart();

            // Update FPS calculation when playing
            if (isPlaying && isInteractiveConsole)
            {
                frameCount++;
                var elapsed = fpsStopwatch.Elapsed.TotalSeconds;
                if (elapsed >= 0.5) // Update FPS every 0.5 seconds
                {
                    currentFps = frameCount / elapsed;
                    frameCount = 0;
                    fpsStopwatch.Restart();
                }
            }

            if (isInteractiveConsole)
            {
                // Render using differential updates
                var elapsed = wallClock.Elapsed;
                RenderWithDiff(renderer, topology, timeline.Current, previousGeneration, BoardStartRow, generation, isPlaying, isPlaying ? currentFps : null, isPlaying ? elapsed : null);
            }
            else
            {
                // Non-interactive mode: use traditional rendering
                await _output.WriteLineAsync($"Generation: {generation}");
                await _output.WriteLineAsync();
                renderer.Render(topology, timeline.Current);
                await _output.WriteLineAsync();
                await _output.WriteLineAsync("Space/Enter: Next | P: Play | Q/Esc: Quit");
            }

            // Check max generations
            if (_options.MaxGenerations is { } maxGen && generation >= maxGen)
            {
                if (!isInteractiveConsole)
                {
                    await _output.WriteLineAsync();
                    await _output.WriteLineAsync($"Reached maximum generations ({maxGen}).");
                }

                break;
            }

            // Handle input
            var isInteractiveInput = ReferenceEquals(_input, System.Console.In) &&
                                     !System.Console.IsInputRedirected;

            if (isInteractiveInput)
            {
                if (isPlaying)
                {
                    // Playing mode: check for keys without blocking
                    if (System.Console.KeyAvailable)
                    {
                        var key = System.Console.ReadKey(true);
                        if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                        {
                            return 0;
                        }

                        if (key.Key is ConsoleKey.P or ConsoleKey.Spacebar)
                        {
                            // Pause
                            isPlaying = false;
                            wallClock.Stop();
                            continue; // Re-render with updated controls
                        }
                    }
                }
                else
                {
                    // Step mode: wait for keypress
                    while (!System.Console.KeyAvailable)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return 0;
                        }

                        await Task.Delay(1, cancellationToken);
                    }

                    var key = System.Console.ReadKey(true);
                    if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                    {
                        return 0;
                    }

                    if (key.Key is ConsoleKey.P)
                    {
                        // Start playing
                        isPlaying = true;
                        wallClock.Start();
                        fpsStopwatch.Restart();
                        frameCount = 0;
                        continue; // Re-render with updated controls, then start advancing
                    }

                    // Space or Enter advances to next generation
                    if (key.Key is not ConsoleKey.Spacebar and not ConsoleKey.Enter)
                    {
                        continue; // Ignore other keys
                    }
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

            // Store current generation before advancing
            previousGeneration = timeline.Current;

            // Advance to next generation
            timeline.Step();
            generation++;

            // Frame rate cap - spin-wait for remaining time after all work is done
            if (isPlaying && isInteractiveInput)
            {
                var minFrameTimeMs = 1000 / _options.MaxFps;
                while (frameStopwatch.ElapsedMilliseconds < minFrameTimeMs)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return 0;
                    }

                    if (System.Console.KeyAvailable)
                    {
                        var key = System.Console.ReadKey(true);
                        if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                        {
                            return 0;
                        }

                        if (key.Key is ConsoleKey.P or ConsoleKey.Spacebar)
                        {
                            // Pause - will take effect next iteration
                            isPlaying = false;
                            wallClock.Stop();
                            break;
                        }
                    }

                    Thread.SpinWait(100);
                }
            }
        }

        return 0;
    }

    private void RenderWithDiff(
        ConsoleRenderer renderer,
        ITopology<Point2D> topology,
        IGeneration<Point2D, bool> currentGeneration,
        IGeneration<Point2D, bool>? previousGeneration,
        int startRow,
        int generationNumber,
        bool isPlaying,
        double? fps = null,
        TimeSpan? elapsed = null)
    {
        // Write header (always at top, reset color first)
        _output.Write("\x1b[0m\x1b[1;1H"); // Reset color, move cursor to top-left
        if (isPlaying && fps.HasValue && elapsed.HasValue)
        {
            var timeStr = $"{(int)elapsed.Value.TotalMinutes:D2}:{elapsed.Value.Seconds:D2}";
            _output.Write($"Generation: {generationNumber}  |  {fps.Value:F1} FPS  |  {timeStr}\x1b[K");
        }
        else
        {
            _output.Write($"Generation: {generationNumber}\x1b[K");
        }

        _output.Write("\n\n"); // Blank line before board

        if (previousGeneration is null)
        {
            // First frame: write full output
            var currentEnumerator = renderer.GetGlyphEnumerator(topology, currentGeneration);
            StreamingDiff.WriteFull(ref currentEnumerator, _output, startRow);
        }
        else
        {
            // Subsequent frames: use differential rendering
            var prevEnumerator = renderer.GetGlyphEnumerator(topology, previousGeneration);
            var currEnumerator = renderer.GetGlyphEnumerator(topology, currentGeneration);
            StreamingDiff.Apply(ref prevEnumerator, ref currEnumerator, _output, startRow);
        }

        // Write controls below the board
        WriteControls(startRow, isPlaying);

        _output.Flush();
    }

    private void WriteControls(int startRow, bool isPlaying)
    {
        // Calculate where controls should go (after board + 1 blank line)
        // Board height = grid height + 2 (borders) + 1 (newline after bottom border)
        var boardHeight = _options.Height + 2;
        var controlsRow = startRow + boardHeight + 1;

        _output.Write($"\x1b[0m\x1b[{controlsRow};1H"); // Reset color, position cursor

        if (isPlaying)
        {
            _output.Write("P/Space: Pause | Q/Esc: Quit");
        }
        else
        {
            _output.Write("Space/Enter: Next | P: Play | Q/Esc: Quit");
        }

        _output.Write("\x1b[K"); // Clear to end of line
    }
}
