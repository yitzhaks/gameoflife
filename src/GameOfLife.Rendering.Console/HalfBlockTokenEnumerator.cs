using GameOfLife.Core;

namespace GameOfLife.Rendering.Console;

/// <summary>
/// A zero-allocation enumerator that yields tokens for half-block console rendering.
/// </summary>
/// <remarks>
/// This ref struct iterates through the packed grid and yields tokens in rendering order:
/// borders, color sequences, and half-block characters. Each output row represents two input rows.
/// Newlines are yielded at the end of each row.
/// </remarks>
public ref struct HalfBlockTokenEnumerator
{
    private readonly IGeneration<Point2D, bool> _generation;
    private readonly HashSet<Point2D> _nodeSet;
    private readonly ConsoleTheme _theme;
    private readonly ViewportRenderer _viewportRenderer;
    private BorderedRenderer _borderedRenderer;

    private int _x;
    private int _y;
    private RenderPhase _phase;
    private AnsiSequence? _lastForegroundColor;
    private AnsiSequence? _lastBackgroundColor;
    private AnsiSequence _pendingForeground;
    private AnsiSequence _pendingBackground;

    private enum RenderPhase
    {
        TopBorder,
        LeftBorder,
        CellForegroundColor,
        CellBackgroundColor,
        CellChar,
        RightBorderReset,
        RightBorder,
        RowNewline,
        BottomBorderReset,
        BottomBorder,
        Done
    }

    /// <summary>
    /// Creates a new half-block token enumerator for the specified layout and generation.
    /// </summary>
    /// <param name="layout">The layout providing positions and packed bounds.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <param name="nodeSet">The set of valid nodes in the topology.</param>
    /// <param name="theme">The rendering theme.</param>
    /// <param name="viewport">Optional viewport for clipping and viewport-aware borders.</param>
    public HalfBlockTokenEnumerator(
        ILayout<Point2D, PackedPoint2D, PackedBounds> layout,
        IGeneration<Point2D, bool> generation,
        HashSet<Point2D> nodeSet,
        ConsoleTheme theme,
        Viewport? viewport = null)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(theme);

        _generation = generation;
        _nodeSet = nodeSet;
        _theme = theme;

        _viewportRenderer = new ViewportRenderer(layout.Bounds, viewport);
        _borderedRenderer = new BorderedRenderer(_viewportRenderer);

        _x = _viewportRenderer.RenderStartX;
        _y = _viewportRenderer.RenderStartY;
        _phase = theme.ShowBorder ? RenderPhase.TopBorder : RenderPhase.CellForegroundColor;
        _lastForegroundColor = null;
        _lastBackgroundColor = null;
        _pendingForeground = default;
        _pendingBackground = default;
        Current = default;
    }

    /// <summary>
    /// Gets the current token.
    /// </summary>
    public Token Current { get; private set; }

    /// <summary>
    /// Advances the enumerator to the next token.
    /// </summary>
    /// <returns>True if there is a next token; false if the enumeration is complete.</returns>
    public bool MoveNext()
    {
        while (true)
        {
            switch (_phase)
            {
                case RenderPhase.TopBorder:
                    if (MoveNextTopBorder())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.LeftBorder:
                    if (MoveNextLeftBorder())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.CellForegroundColor:
                    if (MoveNextCellForegroundColor())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.CellBackgroundColor:
                    if (MoveNextCellBackgroundColor())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.CellChar:
                    if (MoveNextCellChar())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.RightBorderReset:
                    if (MoveNextRightBorderReset())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.RightBorder:
                    if (MoveNextRightBorder())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.RowNewline:
                    if (MoveNextRowNewline())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.BottomBorderReset:
                    if (MoveNextBottomBorderReset())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.BottomBorder:
                    if (MoveNextBottomBorder())
                    {
                        return true;
                    }

                    break;

                case RenderPhase.Done:
                default:
                    return false;
            }
        }
    }

    private bool MoveNextTopBorder()
    {
        if (_borderedRenderer.TryGetTopBorderToken(out Token token))
        {
            Current = token;

            // Check if we just emitted the newline (transition to next phase)
            if (token.IsCharacter && token.Character == '\n')
            {
                _phase = RenderPhase.LeftBorder;
            }

            // Always sync color state back after border tokens
            _lastForegroundColor = _borderedRenderer.LastColor;
            return true;
        }

        return false;
    }

    private bool MoveNextLeftBorder()
    {
        if (!_theme.ShowBorder)
        {
            _phase = RenderPhase.CellForegroundColor;
            return false;
        }

        // Sync color state TO border at start of phase
        _borderedRenderer.SetLastColor(_lastForegroundColor);

        if (_borderedRenderer.TryGetLeftBorderToken(out Token token))
        {
            Current = token;

            // Check if we emitted the vertical bar (transition to cells)
            if (token.IsCharacter && token.Character != '\n')
            {
                _phase = RenderPhase.CellForegroundColor;
            }

            // Always sync color state back after border tokens
            _lastForegroundColor = _borderedRenderer.LastColor;
            return true;
        }

        return false;
    }

    private bool MoveNextCellForegroundColor()
    {
        // Determine the colors for the current cell pair
        Point2D topPoint = (_x, _y * 2);
        Point2D bottomPoint = (_x, (_y * 2) + 1);

        bool topInTopology = _nodeSet.Contains(topPoint);
        bool bottomInTopology = _nodeSet.Contains(bottomPoint);

        if (!topInTopology && !bottomInTopology)
        {
            // Points not in topology - render as space with dark gray foreground and background
            _pendingForeground = AnsiSequence.ForegroundDarkGray;
            _pendingBackground = AnsiSequence.BackgroundDarkGray;
        }
        else
        {
            bool topAlive = topInTopology && _generation[topPoint];
            bool bottomAlive = bottomInTopology && _generation[bottomPoint];

            // Determine colors based on half-block character mapping
            if (topAlive && bottomAlive)
            {
                // Both alive: Full block with green foreground, default background
                _pendingForeground = AnsiSequence.ForegroundGreen;
                _pendingBackground = AnsiSequence.BackgroundDefault;
            }
            else if (topAlive)
            {
                // Top alive, bottom dead: Upper half block with green foreground, dark gray background
                _pendingForeground = AnsiSequence.ForegroundGreen;
                _pendingBackground = AnsiSequence.BackgroundDarkGray;
            }
            else if (bottomAlive)
            {
                // Top dead, bottom alive: Lower half block with green foreground, dark gray background
                _pendingForeground = AnsiSequence.ForegroundGreen;
                _pendingBackground = AnsiSequence.BackgroundDarkGray;
            }
            else
            {
                // Both dead: Space with dark gray foreground and background
                _pendingForeground = AnsiSequence.ForegroundDarkGray;
                _pendingBackground = AnsiSequence.BackgroundDarkGray;
            }
        }

        if (TryEmitForegroundColor(_pendingForeground))
        {
            _phase = RenderPhase.CellBackgroundColor;
            return true;
        }

        _phase = RenderPhase.CellBackgroundColor;
        return false;
    }

    private bool MoveNextCellBackgroundColor()
    {
        if (TryEmitBackgroundColor(_pendingBackground))
        {
            _phase = RenderPhase.CellChar;
            return true;
        }

        _phase = RenderPhase.CellChar;
        return false;
    }

    private bool MoveNextCellChar()
    {
        Point2D topPoint = (_x, _y * 2);
        Point2D bottomPoint = (_x, (_y * 2) + 1);

        bool topInTopology = _nodeSet.Contains(topPoint);
        bool bottomInTopology = _nodeSet.Contains(bottomPoint);

        char character;

        if (!topInTopology && !bottomInTopology)
        {
            character = ' ';
        }
        else
        {
            bool topAlive = topInTopology && _generation[topPoint];
            bool bottomAlive = bottomInTopology && _generation[bottomPoint];

            if (topAlive && bottomAlive)
            {
                character = ConsoleTheme.HalfBlock.Full;
            }
            else if (topAlive)
            {
                character = ConsoleTheme.HalfBlock.UpperHalf;
            }
            else if (bottomAlive)
            {
                character = ConsoleTheme.HalfBlock.LowerHalf;
            }
            else
            {
                character = ' ';
            }
        }

        Current = Token.Char(character);
        _x++;

        if (_x > _viewportRenderer.RenderEndX)
        {
            _phase = _theme.ShowBorder ? RenderPhase.RightBorderReset : RenderPhase.RowNewline;
        }
        else
        {
            _phase = RenderPhase.CellForegroundColor;
        }

        return true;
    }

    private bool MoveNextRightBorderReset()
    {
        // Reset background to default before rendering border characters
        if (TryEmitBackgroundColor(AnsiSequence.BackgroundDefault))
        {
            _phase = RenderPhase.RightBorder;
            return true;
        }

        _phase = RenderPhase.RightBorder;
        return false;
    }

    private bool MoveNextRightBorder()
    {
        // Synchronize foreground color state
        _borderedRenderer.SetLastColor(_lastForegroundColor);

        if (_borderedRenderer.TryGetRightBorderToken(out Token token))
        {
            Current = token;

            // Check if we emitted the vertical bar (transition to newline)
            if (token.IsCharacter && token.Character != '\n')
            {
                _phase = RenderPhase.RowNewline;
            }

            // Always sync color state back after border tokens
            _lastForegroundColor = _borderedRenderer.LastColor;
            return true;
        }

        return false;
    }

    private bool MoveNextRowNewline()
    {
        Current = WellKnownTokens.Newline;
        _y++;

        if (_y > _viewportRenderer.RenderEndY)
        {
            _phase = _theme.ShowBorder ? RenderPhase.BottomBorderReset : RenderPhase.Done;
        }
        else
        {
            _x = _viewportRenderer.RenderStartX;
            _phase = _theme.ShowBorder ? RenderPhase.LeftBorder : RenderPhase.CellForegroundColor;
        }

        return true;
    }

    private bool MoveNextBottomBorderReset()
    {
        // Reset background to default before rendering border characters
        if (TryEmitBackgroundColor(AnsiSequence.BackgroundDefault))
        {
            _phase = RenderPhase.BottomBorder;
            return true;
        }

        _phase = RenderPhase.BottomBorder;
        return false;
    }

    private bool MoveNextBottomBorder()
    {
        // Synchronize foreground color state
        _borderedRenderer.SetLastColor(_lastForegroundColor);

        if (_borderedRenderer.TryGetBottomBorderToken(out Token token, out bool isComplete))
        {
            Current = token;
            _lastForegroundColor = _borderedRenderer.LastColor;

            if (isComplete)
            {
                _phase = RenderPhase.Done;
            }

            return true;
        }

        return false;
    }

    private bool TryEmitForegroundColor(AnsiSequence color)
    {
        if (_lastForegroundColor != color)
        {
            Current = Token.Ansi(color);
            _lastForegroundColor = color;
            return true;
        }

        return false;
    }

    private bool TryEmitBackgroundColor(AnsiSequence color)
    {
        if (_lastBackgroundColor != color)
        {
            Current = Token.Ansi(color);
            _lastBackgroundColor = color;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns this enumerator (supports foreach).
    /// </summary>
    public readonly HalfBlockTokenEnumerator GetEnumerator() => this;
}
