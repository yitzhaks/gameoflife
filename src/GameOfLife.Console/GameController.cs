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

        // Create viewport if board is larger than console
        Viewport? viewport = null;
        var viewportHeight = _options.Height;

        if (isInteractiveConsole)
        {
            // Account for borders (2 chars each side) and header/controls (4 rows reserved)
            var availableWidth = System.Console.WindowWidth - 2;
            var availableHeight = System.Console.WindowHeight - 6; // Header(2) + Controls(2) + margins

            if (_options.Width > availableWidth || _options.Height > availableHeight)
            {
                var viewportWidth = Math.Min(_options.Width, availableWidth);
                viewportHeight = Math.Min(_options.Height, availableHeight);
                viewport = new Viewport(viewportWidth, viewportHeight, _options.Width, _options.Height);
            }
        }

        // Run game loop
        int generation = 0;

        // Double-buffered frame storage for differential rendering
        List<Glyph>[] frameBuffers = [[], []];
        int currentBufferIndex = 0;

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
                // Render using differential updates against frame buffer
                var elapsed = wallClock.Elapsed;
                var prevBuffer = frameBuffers[currentBufferIndex];
                var currBuffer = frameBuffers[1 - currentBufferIndex];
                RenderWithDiff(renderer, topology, timeline.Current, prevBuffer, currBuffer, BoardStartRow, generation, isPlaying, viewport, viewportHeight, isPlaying ? currentFps : null, isPlaying ? elapsed : null);
                currentBufferIndex = 1 - currentBufferIndex; // Swap buffers
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

                        // Handle viewport navigation during autoplay - drain all pending nav keys
                        if (viewport is not null && HandleViewportNavigation(key, viewport))
                        {
                            DrainViewportNavigation(viewport);
                            continue;
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

                    // Handle viewport navigation - drain all pending nav keys
                    if (viewport is not null && HandleViewportNavigation(key, viewport))
                    {
                        DrainViewportNavigation(viewport);
                        continue;
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

                        // Handle viewport navigation during frame wait - drain all pending nav keys
                        if (viewport is not null && HandleViewportNavigation(key, viewport))
                        {
                            DrainViewportNavigation(viewport);
                            break; // Re-render immediately
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
        List<Glyph> previousFrameBuffer,
        List<Glyph> currentFrameBuffer,
        int startRow,
        int generationNumber,
        bool isPlaying,
        Viewport? viewport,
        int viewportHeight,
        double? fps = null,
        TimeSpan? elapsed = null)
    {
        // Write header (always at top, reset color first)
        _output.Write("\x1b[0m\x1b[1;1H"); // Reset color, move cursor to top-left

        var headerParts = new List<string> { $"Generation: {generationNumber}" };

        if (isPlaying && fps.HasValue && elapsed.HasValue)
        {
            var timeStr = $"{(int)elapsed.Value.TotalMinutes:D2}:{elapsed.Value.Seconds:D2}";
            headerParts.Add($"{fps.Value:F1} FPS");
            headerParts.Add(timeStr);
        }

        if (viewport is not null)
        {
            headerParts.Add($"View: ({viewport.OffsetX},{viewport.OffsetY})");
        }

        _output.Write(string.Join("  |  ", headerParts));
        _output.Write("\x1b[K"); // Clear to end of line

        _output.Write("\n\n"); // Blank line before board

        var currentEnumerator = renderer.GetGlyphEnumerator(topology, currentGeneration, viewport);

        if (previousFrameBuffer.Count == 0)
        {
            // First frame: write full output and capture to buffer
            StreamingDiff.WriteFullAndCapture(ref currentEnumerator, _output, currentFrameBuffer, startRow);
        }
        else
        {
            // Subsequent frames: diff against previous frame buffer
            StreamingDiff.ApplyAndCapture(previousFrameBuffer, ref currentEnumerator, _output, currentFrameBuffer, startRow);
        }

        // Write controls below the board
        WriteControls(startRow, isPlaying, viewport, viewportHeight);

        _output.Flush();
    }

    private void WriteControls(int startRow, bool isPlaying, Viewport? viewport, int viewportHeight)
    {
        // Calculate where controls should go (after board + 1 blank line)
        // Board height = viewport height (or grid height) + 2 (borders)
        var boardHeight = viewportHeight + 2;
        var controlsRow = startRow + boardHeight + 1;

        _output.Write($"\x1b[0m\x1b[{controlsRow};1H"); // Reset color, position cursor

        var controls = new List<string>();

        if (viewport is not null)
        {
            controls.Add("WASD/Arrows: Pan");
        }

        if (isPlaying)
        {
            controls.Add("P/Space: Pause");
        }
        else
        {
            controls.Add("Space/Enter: Next");
            controls.Add("P: Play");
        }

        controls.Add("Q/Esc: Quit");

        _output.Write(string.Join(" | ", controls));
        _output.Write("\x1b[K"); // Clear to end of line
    }

    private static bool HandleViewportNavigation(ConsoleKeyInfo key, Viewport viewport)
    {
        var (deltaX, deltaY) = GetNavigationDelta(key.Key);

        if (deltaX == 0 && deltaY == 0)
        {
            return false;
        }

        // Track position before move to detect if it actually changed
        var oldX = viewport.OffsetX;
        var oldY = viewport.OffsetY;

        viewport.Move(deltaX, deltaY);

        // Only return true if the viewport actually moved
        return viewport.OffsetX != oldX || viewport.OffsetY != oldY;
    }

    private static void DrainViewportNavigation(Viewport viewport)
    {
        // Process all pending navigation keys to prevent input lag
        while (System.Console.KeyAvailable)
        {
            var key = System.Console.ReadKey(true);
            var (deltaX, deltaY) = GetNavigationDelta(key.Key);

            if (deltaX == 0 && deltaY == 0)
            {
                // Non-navigation key - stop draining so it can be processed normally
                // Unfortunately we can't "unread" it, so it's lost
                // But this is better than hanging
                break;
            }

            viewport.Move(deltaX, deltaY);
        }
    }

    private static (int deltaX, int deltaY) GetNavigationDelta(ConsoleKey key)
    {
        return key switch
        {
            ConsoleKey.W or ConsoleKey.UpArrow => (0, -1),
            ConsoleKey.S or ConsoleKey.DownArrow => (0, 1),
            ConsoleKey.A or ConsoleKey.LeftArrow => (-1, 0),
            ConsoleKey.D or ConsoleKey.RightArrow => (1, 0),
            _ => (0, 0)
        };
    }
}
