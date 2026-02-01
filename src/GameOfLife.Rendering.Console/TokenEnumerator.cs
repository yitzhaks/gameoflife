using GameOfLife.Core;

namespace GameOfLife.Rendering.Console;

/// <summary>
/// A zero-allocation enumerator that yields tokens for console rendering.
/// </summary>
/// <remarks>
/// This ref struct iterates through the grid and yields tokens in rendering order:
/// borders, color sequences, and characters. Newlines are yielded at the end of each row.
/// </remarks>
public ref struct TokenEnumerator
{
    private readonly IGeneration<Point2D, bool> _generation;
    private readonly HashSet<Point2D> _nodeSet;
    private readonly ConsoleTheme _theme;
    private readonly ViewportRenderer _viewportRenderer;
    private BorderedRenderer _borderedRenderer;

    private int _x;
    private int _y;
    private RenderPhase _phase;
    private AnsiSequence? _lastColor;

    private enum RenderPhase
    {
        TopBorder,
        LeftBorder,
        CellColor,
        CellChar,
        RightBorder,
        RowNewline,
        BottomBorder,
        Done
    }

    /// <summary>
    /// Creates a new token enumerator for the specified layout and generation.
    /// </summary>
    /// <param name="layout">The layout providing positions and bounds.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <param name="nodeSet">The set of valid nodes in the topology.</param>
    /// <param name="theme">The rendering theme.</param>
    /// <param name="viewport">Optional viewport for clipping and viewport-aware borders.</param>
    public TokenEnumerator(
        ILayout<Point2D, Point2D, RectangularBounds> layout,
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
        _phase = theme.ShowBorder ? RenderPhase.TopBorder : RenderPhase.CellColor;
        _lastColor = null;
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

                case RenderPhase.CellColor:
                    if (MoveNextCellColor())
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
            _lastColor = _borderedRenderer.LastColor;
            return true;
        }

        return false;
    }

    private bool MoveNextLeftBorder()
    {
        if (!_theme.ShowBorder)
        {
            _phase = RenderPhase.CellColor;
            return false;
        }

        // Sync color state TO border at start of phase
        _borderedRenderer.SetLastColor(_lastColor);

        if (_borderedRenderer.TryGetLeftBorderToken(out Token token))
        {
            Current = token;

            // Check if we emitted the vertical bar (transition to cells)
            if (token.IsCharacter && token.Character != '\n')
            {
                _phase = RenderPhase.CellColor;
            }

            // Always sync color state back after border tokens
            _lastColor = _borderedRenderer.LastColor;
            return true;
        }

        return false;
    }

    private bool MoveNextCellColor()
    {
        // Determine the color for the current cell
        Point2D point = (_x, _y);
        AnsiSequence targetColor;

        if (_nodeSet.Contains(point))
        {
            bool isAlive = _generation[point];
            targetColor = isAlive ? AnsiSequence.ForegroundGreen : AnsiSequence.ForegroundDarkGray;
        }
        else
        {
            // Points not in topology get no color change, just a space
            _phase = RenderPhase.CellChar;
            return false;
        }

        if (TryEmitColor(targetColor))
        {
            _phase = RenderPhase.CellChar;
            return true;
        }

        _phase = RenderPhase.CellChar;
        return false;
    }

    private bool MoveNextCellChar()
    {
        Point2D point = (_x, _y);
        char character;

        if (_nodeSet.Contains(point))
        {
            bool isAlive = _generation[point];
            character = isAlive ? _theme.AliveChar : _theme.DeadChar;
        }
        else
        {
            character = ' ';
        }

        Current = Token.Char(character);
        _x++;

        if (_x > _viewportRenderer.RenderEndX)
        {
            _phase = _theme.ShowBorder ? RenderPhase.RightBorder : RenderPhase.RowNewline;
        }
        else
        {
            _phase = RenderPhase.CellColor;
        }

        return true;
    }

    private bool MoveNextRightBorder()
    {
        // Synchronize color state
        _borderedRenderer.SetLastColor(_lastColor);

        if (_borderedRenderer.TryGetRightBorderToken(out Token token))
        {
            Current = token;

            // Check if we emitted the vertical bar (transition to newline)
            if (token.IsCharacter && token.Character != '\n')
            {
                _phase = RenderPhase.RowNewline;
            }

            // Always sync color state back after border tokens
            _lastColor = _borderedRenderer.LastColor;
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
            _phase = _theme.ShowBorder ? RenderPhase.BottomBorder : RenderPhase.Done;
        }
        else
        {
            _x = _viewportRenderer.RenderStartX;
            _phase = _theme.ShowBorder ? RenderPhase.LeftBorder : RenderPhase.CellColor;
        }

        return true;
    }

    private bool MoveNextBottomBorder()
    {
        // Synchronize color state
        _borderedRenderer.SetLastColor(_lastColor);

        if (_borderedRenderer.TryGetBottomBorderToken(out Token token, out bool isComplete))
        {
            Current = token;
            _lastColor = _borderedRenderer.LastColor;

            if (isComplete)
            {
                _phase = RenderPhase.Done;
            }

            return true;
        }

        return false;
    }

    private bool TryEmitColor(AnsiSequence color)
    {
        if (_lastColor != color)
        {
            Current = Token.Ansi(color);
            _lastColor = color;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns this enumerator (supports foreach).
    /// </summary>
    public readonly TokenEnumerator GetEnumerator() => this;
}
