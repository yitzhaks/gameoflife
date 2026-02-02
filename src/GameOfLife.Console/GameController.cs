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
    private readonly HexShapeLoader _hexShapeLoader;
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
    /// <param name="shapeLoader">The shape loader for loading rectangular patterns.</param>
    /// <param name="hexShapeLoader">The shape loader for loading hex patterns.</param>
    /// <param name="output">The text writer for output.</param>
    /// <param name="input">The text reader for input.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public GameController(
        CommandLineOptions options,
        ShapeLoader shapeLoader,
        HexShapeLoader hexShapeLoader,
        TextWriter output,
        TextReader input)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(shapeLoader);
        ArgumentNullException.ThrowIfNull(hexShapeLoader);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(input);

        _options = options;
        _shapeLoader = shapeLoader;
        _hexShapeLoader = hexShapeLoader;
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
        bool isInteractiveConsole = ReferenceEquals(_output, System.Console.Out) &&
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

            // Run game loop on dedicated thread to avoid blocking threadpool with SpinWait
            return await Task.Factory.StartNew(
                () => RunGameLoop(isInteractiveConsole, cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
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

    private int RunGameLoop(bool isInteractiveConsole, CancellationToken cancellationToken)
    {
        // Check for pattern analysis mode
        if (_options.AnalyzePatterns)
        {
            PatternAnalyzer.AnalyzePatterns(_output);
            return 0;
        }

        // Branch based on topology type
        if (_options.Topology == BoardTopology.Hexagonal)
        {
            return RunHexGameLoop(isInteractiveConsole, cancellationToken);
        }

        return RunRectangularGameLoop(isInteractiveConsole, cancellationToken);
    }

    private int RunRectangularGameLoop(bool isInteractiveConsole, CancellationToken cancellationToken)
    {
        // Validate even height for half-block mode
        if (_options.AspectMode == AspectMode.HalfBlock && _options.Height % 2 != 0)
        {
            _output.WriteLine("Error: Half-block mode requires even height.");
            return 1;
        }

        Size2D boardSize = (_options.Width, _options.Height);

        // Create initial generation with injected shapes
        IGeneration<Point2D, bool> initialGeneration;
        {
            using var builder = new RectangularGenerationBuilder(boardSize);
            foreach (ShapeInjection injection in _options.Injections)
            {
                IReadOnlyList<Point2D> pattern = _shapeLoader.LoadPattern(injection.PatternName);
                foreach (Point2D point in pattern)
                {
                    Point2D target = injection.Position + point;

                    // Clip points outside bounds
                    if (target.IsInBounds(boardSize))
                    {
                        builder[target] = true;
                    }
                }
            }

            // Build() takes ownership of the pooled array
            initialGeneration = builder.Build();
        }

        // Create rectangular world and timeline for optimized performance
        var world = new RectangularWorld(boardSize);
        // the timeline takes ownership of the pooled array in initialGeneration
        using var timeline = Timeline.Create(world, initialGeneration);

        // Create renderer
        var layoutEngine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(_output, layoutEngine, theme);

        // Create viewport if board is larger than console
        Viewport? viewport = null;

        // In half-block mode, board height is halved for display purposes
        int effectiveBoardHeight = _options.AspectMode == AspectMode.HalfBlock
            ? _options.Height / 2
            : _options.Height;
        int viewportHeight = effectiveBoardHeight;

        if (isInteractiveConsole)
        {
            // Account for borders (2 chars each side) and header/controls (4 rows reserved)
            int availableWidth = System.Console.WindowWidth - 2;
            int availableHeight = System.Console.WindowHeight - 6; // Header(2) + Controls(2) + margins

            if (_options.Width > availableWidth || effectiveBoardHeight > availableHeight)
            {
                int viewportWidth = Math.Min(_options.Width, availableWidth);
                viewportHeight = Math.Min(effectiveBoardHeight, availableHeight);
                viewport = new Viewport(viewportWidth, viewportHeight, _options.Width, effectiveBoardHeight);
            }
        }

        // Run game loop
        int generation = 0;

        // Double-buffered frame storage for differential rendering (pre-allocated)
        int bufferHeight = viewport?.Height ?? effectiveBoardHeight;
        int bufferWidth = viewport?.Width ?? _options.Width;
        FrameBuffer[] frameBuffers = [
            FrameBuffer.ForViewport(bufferWidth, bufferHeight),
            FrameBuffer.ForViewport(bufferWidth, bufferHeight)
        ];
        int currentBufferIndex = 0;

        // Header takes 2 lines (Generation: X + blank line), so board starts at row 3
        const int BoardStartRow = 3;

        // FPS tracking for play mode
        var fpsStopwatch = Stopwatch.StartNew();
        int frameCount = 0;
        double currentFps = 0.0;

        // Wall clock for elapsed time display
        var wallClock = Stopwatch.StartNew();

        // Play/pause state (starts based on command-line option)
        bool isPlaying = _options.StartAutoplay;

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
                double elapsed = fpsStopwatch.Elapsed.TotalSeconds;
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
                TimeSpan elapsed = wallClock.Elapsed;
                FrameBuffer prevBuffer = frameBuffers[currentBufferIndex];
                FrameBuffer currBuffer = frameBuffers[1 - currentBufferIndex];
                RenderWithDiff(renderer, world.Topology, timeline.Current, prevBuffer, currBuffer, BoardStartRow, generation, isPlaying, viewport, viewportHeight, isPlaying ? currentFps : null, isPlaying ? elapsed : null);
                currentBufferIndex = 1 - currentBufferIndex; // Swap buffers
            }
            else
            {
                // Non-interactive mode: use traditional rendering
                _output.WriteLine($"Generation: {generation}");
                _output.WriteLine();
                renderer.Render(world.Topology, timeline.Current);
                _output.WriteLine();
                _output.WriteLine("Space/Enter: Next | P: Play | Q/Esc: Quit");
            }

            // Check max generations
            if (_options.MaxGenerations is { } maxGen && generation >= maxGen)
            {
                if (!isInteractiveConsole)
                {
                    _output.WriteLine();
                    _output.WriteLine($"Reached maximum generations ({maxGen}).");
                }

                break;
            }

            // Handle input
            bool isInteractiveInput = ReferenceEquals(_input, System.Console.In) &&
                                     !System.Console.IsInputRedirected;

            if (isInteractiveInput)
            {
                if (isPlaying)
                {
                    // Playing mode: check for keys without blocking
                    if (System.Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = System.Console.ReadKey(true);
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
                        // Don't use continue here - let the loop proceed to advance generation and respect frame timing
                        if (viewport is not null && HandleViewportNavigation(key, viewport))
                        {
                            DrainViewportNavigation(viewport);
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

                        Thread.Sleep(1);
                    }

                    ConsoleKeyInfo key = System.Console.ReadKey(true);
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
                string? line = _input.ReadLine();
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
                int minFrameTimeMs = 1000 / _options.MaxFps;
                while (frameStopwatch.ElapsedMilliseconds < minFrameTimeMs)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return 0;
                    }

                    if (System.Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = System.Console.ReadKey(true);
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
        RectangularTopology topology,
        IGeneration<Point2D, bool> currentGeneration,
        FrameBuffer previousFrameBuffer,
        FrameBuffer currentFrameBuffer,
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
            string timeStr = $"{(int)elapsed.Value.TotalMinutes:D2}:{elapsed.Value.Seconds:D2}";
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

        if (_options.AspectMode == AspectMode.HalfBlock)
        {
            HalfBlockColorNormalizedGlyphEnumerator currentEnumerator =
                renderer.GetHalfBlockGlyphEnumerator(topology, currentGeneration, viewport);

            if (previousFrameBuffer.Count == 0)
            {
                // First frame: write full output and capture to buffer
                StreamingDiff.WriteFullAndCaptureHalfBlock(ref currentEnumerator, _output, currentFrameBuffer, startRow);
            }
            else
            {
                // Subsequent frames: diff against previous frame buffer
                StreamingDiff.ApplyAndCaptureHalfBlock(previousFrameBuffer, ref currentEnumerator, _output, currentFrameBuffer, startRow);
            }
        }
        else
        {
            ColorNormalizedGlyphEnumerator currentEnumerator =
                renderer.GetGlyphEnumerator(topology, currentGeneration, viewport);

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
        }

        // Write controls below the board
        WriteControls(startRow, isPlaying, viewport, viewportHeight);

        _output.Flush();
    }

    private void WriteControls(int startRow, bool isPlaying, Viewport? viewport, int viewportHeight)
    {
        // Calculate where controls should go (after board + 1 blank line)
        // Board height = viewport height (or grid height) + 2 (borders)
        int boardHeight = viewportHeight + 2;
        int controlsRow = startRow + boardHeight + 1;

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
        (int deltaX, int deltaY) = GetNavigationDelta(key.Key);

        if (deltaX == 0 && deltaY == 0)
        {
            return false;
        }

        // Track position before move to detect if it actually changed
        int oldX = viewport.OffsetX;
        int oldY = viewport.OffsetY;

        viewport.Move(deltaX, deltaY);

        // Only return true if the viewport actually moved
        return viewport.OffsetX != oldX || viewport.OffsetY != oldY;
    }

    private static void DrainViewportNavigation(Viewport viewport)
    {
        // Process all pending navigation keys to prevent input lag
        while (System.Console.KeyAvailable)
        {
            ConsoleKeyInfo key = System.Console.ReadKey(true);
            (int deltaX, int deltaY) = GetNavigationDelta(key.Key);

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
#pragma warning disable IDE0072 // Populate switch - intentionally using default for unhandled keys
        return key switch
        {
            ConsoleKey.W or ConsoleKey.UpArrow => (0, -1),
            ConsoleKey.S or ConsoleKey.DownArrow => (0, 1),
            ConsoleKey.A or ConsoleKey.LeftArrow => (-1, 0),
            ConsoleKey.D or ConsoleKey.RightArrow => (1, 0),
            _ => default
        };
#pragma warning restore IDE0072
    }

    private int RunHexGameLoop(bool isInteractiveConsole, CancellationToken cancellationToken)
    {
        int radius = _options.HexRadius;
        var topology = new HexagonalTopology(radius);

        // Create hexagonal world with selected rules
        ICellularAutomatonRules rules = _options.HexRules.ToUpperInvariant() switch
        {
            "B2S34" => new HexRulesB2S34(),
            "B2S35" => new HexRulesB2S35(),
            "B24S35" => new HexRulesB24S35(),
            "B2S23" => new HexRulesB2S23(),
            "CLASSIC" => new ClassicRules(),
            _ => new HexRulesB2S34()
        };
        var world = new HexagonalWorld(radius, rules);

        // Create renderer
        var layoutEngine = new HexLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var hexRenderer = new HexConsoleRenderer(_output, layoutEngine, theme);

        // Run game loop
        int generation = 0;

        // Create initial generation with injected shapes and/or random fill
        IGeneration<HexPoint, bool>? currentGeneration = null;
        try
        {
            var aliveCells = new HashSet<HexPoint>();

            // Apply hex shape injections
            foreach (HexShapeInjection injection in _options.HexInjections)
            {
                IReadOnlyList<HexPoint> pattern = _hexShapeLoader.LoadPattern(injection.PatternName);
                aliveCells.UnionWith(
                    pattern
                        .Select(point => injection.Position + point)
                        .Where(target => target.IsWithinRadius(radius)));
            }

            // Apply random fill if specified
            if (_options.HexFillPercent > 0)
            {
#pragma warning disable CA5394 // Random is used for game simulation, not security
                var random = new Random();
                foreach (HexPoint cell in topology.Nodes)
                {
                    if (random.Next(100) < _options.HexFillPercent)
                    {
                        _ = aliveCells.Add(cell);
                    }
                }
#pragma warning restore CA5394
            }

            currentGeneration = new HexGeneration(aliveCells);

            // Play/pause state
            bool isPlaying = _options.StartAutoplay;

            // Frame timing for FPS cap
            var frameStopwatch = new Stopwatch();

            // FPS tracking
            var fpsStopwatch = Stopwatch.StartNew();
            int frameCount = 0;
            double currentFps = 0.0;

            // Wall clock for elapsed time
            var wallClock = Stopwatch.StartNew();

            while (!cancellationToken.IsCancellationRequested)
            {
                frameStopwatch.Restart();

                // Update FPS when playing
                if (isPlaying && isInteractiveConsole)
                {
                    frameCount++;
                    double elapsed = fpsStopwatch.Elapsed.TotalSeconds;
                    if (elapsed >= 0.5)
                    {
                        currentFps = frameCount / elapsed;
                        frameCount = 0;
                        fpsStopwatch.Restart();
                    }
                }

                if (isInteractiveConsole)
                {
                    // Render with header
                    _output.Write("\x1b[0m\x1b[1;1H"); // Reset color, move to top-left

                    var headerParts = new List<string>
                {
                    $"Generation: {generation}",
                    $"Hex r={radius} {_options.HexRules}"
                };

                    if (isPlaying)
                    {
                        string timeStr = $"{(int)wallClock.Elapsed.TotalMinutes:D2}:{wallClock.Elapsed.Seconds:D2}";
                        headerParts.Add($"{currentFps:F1} FPS");
                        headerParts.Add(timeStr);
                    }

                    _output.Write(string.Join("  |  ", headerParts));
                    _output.Write("\x1b[K\n\n"); // Clear to end and add blank line

                    hexRenderer.Render(topology, currentGeneration);

                    // Write controls
                    _output.Write("\x1b[0m\n");
                    if (isPlaying)
                    {
                        _output.Write("P/Space: Pause | Q/Esc: Quit");
                    }
                    else
                    {
                        _output.Write("Space/Enter: Next | P: Play | Q/Esc: Quit");
                    }

                    _output.Write("\x1b[K");
                    _output.Flush();
                }
                else
                {
                    _output.WriteLine($"Generation: {generation}");
                    _output.WriteLine();
                    hexRenderer.Render(topology, currentGeneration);
                    _output.WriteLine();
                    _output.WriteLine("Space/Enter: Next | P: Play | Q/Esc: Quit");
                }

                // Check max generations
                if (_options.MaxGenerations is { } maxGen && generation >= maxGen)
                {
                    if (!isInteractiveConsole)
                    {
                        _output.WriteLine();
                        _output.WriteLine($"Reached maximum generations ({maxGen}).");
                    }

                    break;
                }

                // Handle input
                bool isInteractiveInput = ReferenceEquals(_input, System.Console.In) &&
                                         !System.Console.IsInputRedirected;

                if (isInteractiveInput)
                {
                    if (isPlaying)
                    {
                        if (System.Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = System.Console.ReadKey(true);
                            if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                            {
                                return 0;
                            }

                            if (key.Key is ConsoleKey.P or ConsoleKey.Spacebar)
                            {
                                isPlaying = false;
                                wallClock.Stop();
                                continue;
                            }
                        }
                    }
                    else
                    {
                        while (!System.Console.KeyAvailable)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return 0;
                            }

                            Thread.Sleep(1);
                        }

                        ConsoleKeyInfo key = System.Console.ReadKey(true);
                        if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                        {
                            return 0;
                        }

                        if (key.Key is ConsoleKey.P)
                        {
                            isPlaying = true;
                            wallClock.Start();
                            fpsStopwatch.Restart();
                            frameCount = 0;
                            continue;
                        }

                        if (key.Key is not ConsoleKey.Spacebar and not ConsoleKey.Enter)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    string? line = _input.ReadLine();
                    if (line is null || line.Trim().Equals("q", StringComparison.OrdinalIgnoreCase))
                    {
                        return 0;
                    }
                }

                // Advance generation
                IGeneration<HexPoint, bool> nextGeneration = world.Tick(currentGeneration);
                currentGeneration.Dispose();
                currentGeneration = nextGeneration;
                generation++;

                // Frame rate cap
                if (isPlaying && isInteractiveInput)
                {
                    int minFrameTimeMs = 1000 / _options.MaxFps;
                    while (frameStopwatch.ElapsedMilliseconds < minFrameTimeMs)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return 0;
                        }

                        if (System.Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = System.Console.ReadKey(true);
                            if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                            {
                                return 0;
                            }

                            if (key.Key is ConsoleKey.P or ConsoleKey.Spacebar)
                            {
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
        finally
        {
            currentGeneration?.Dispose();
        }
    }
}
