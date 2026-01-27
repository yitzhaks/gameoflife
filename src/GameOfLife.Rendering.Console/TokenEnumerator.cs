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
    private readonly RectangularBounds _bounds;
    private readonly Viewport? _viewport;
    private readonly int _width;
    private readonly int _renderStartX;
    private readonly int _renderStartY;
    private readonly int _renderEndX;
    private readonly int _renderEndY;

    private int _x;
    private int _y;
    private RenderPhase _phase;
    private int _borderPosition;
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
        _generation = generation;
        _nodeSet = nodeSet;
        _theme = theme;
        _bounds = layout.Bounds;
        _viewport = viewport;

        // Calculate render bounds based on viewport
        if (viewport is not null)
        {
            _renderStartX = _bounds.Min.X + viewport.OffsetX;
            _renderStartY = _bounds.Min.Y + viewport.OffsetY;
            _renderEndX = Math.Min(_renderStartX + viewport.Width - 1, _bounds.Max.X);
            _renderEndY = Math.Min(_renderStartY + viewport.Height - 1, _bounds.Max.Y);
            _width = _renderEndX - _renderStartX + 1;
        }
        else
        {
            _renderStartX = _bounds.Min.X;
            _renderStartY = _bounds.Min.Y;
            _renderEndX = _bounds.Max.X;
            _renderEndY = _bounds.Max.Y;
            _width = _bounds.Max.X - _bounds.Min.X + 1;
        }

        _x = _renderStartX;
        _y = _renderStartY;
        _phase = theme.ShowBorder ? RenderPhase.TopBorder : RenderPhase.CellColor;
        _borderPosition = 0;
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
                    return false;
            }
        }
    }

    private bool MoveNextTopBorder()
    {
        // Top border: color, left corner, horizontals, right corner, newline
        var isAtTop = _viewport?.IsAtTop ?? true;
        var isAtLeft = _viewport?.IsAtLeft ?? true;
        var isAtRight = _viewport?.IsAtRight ?? true;
        var borderColor = (_viewport is not null && !isAtTop) ? AnsiSequence.ForegroundDarkGray : AnsiSequence.ForegroundGray;

        if (_borderPosition == 0)
        {
            // Border color
            if (TryEmitColor(borderColor))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            // Top left corner
            Current = Token.Char(GetTopLeftCorner(isAtTop, isAtLeft));
            _borderPosition++;
            return true;
        }

        if (_borderPosition <= _width + 1)
        {
            // Horizontal bars or up arrows
            Current = Token.Char(isAtTop ? ConsoleTheme.Border.Horizontal : ConsoleTheme.ViewportBorder.Up);
            _borderPosition++;
            return true;
        }

        if (_borderPosition == _width + 2)
        {
            // Top right corner
            Current = Token.Char(GetTopRightCorner(isAtTop, isAtRight));
            _borderPosition++;
            return true;
        }

        if (_borderPosition == _width + 3)
        {
            // Newline
            Current = WellKnownTokens.Newline;
            _borderPosition = 0;
            _phase = RenderPhase.LeftBorder;
            return true;
        }

        return false;
    }

    private static char GetTopLeftCorner(bool isAtTop, bool isAtLeft)
    {
        if (isAtTop && isAtLeft)
        {
            return ConsoleTheme.Border.TopLeft;
        }

        if (isAtTop)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        if (isAtLeft)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return ConsoleTheme.ViewportBorder.DiagonalTopLeft;
    }

    private static char GetTopRightCorner(bool isAtTop, bool isAtRight)
    {
        if (isAtTop && isAtRight)
        {
            return ConsoleTheme.Border.TopRight;
        }

        if (isAtTop)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        if (isAtRight)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return ConsoleTheme.ViewportBorder.DiagonalTopRight;
    }

    private bool MoveNextLeftBorder()
    {
        if (!_theme.ShowBorder)
        {
            _phase = RenderPhase.CellColor;
            return false;
        }

        var isAtLeft = _viewport?.IsAtLeft ?? true;
        var borderColor = (_viewport is not null && !isAtLeft) ? AnsiSequence.ForegroundDarkGray : AnsiSequence.ForegroundGray;

        // Left border: color, vertical bar or left arrow
        if (_borderPosition == 0)
        {
            if (TryEmitColor(borderColor))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            Current = Token.Char(isAtLeft ? ConsoleTheme.Border.Vertical : ConsoleTheme.ViewportBorder.Left);
            _borderPosition = 0;
            _phase = RenderPhase.CellColor;
            return true;
        }

        return false;
    }

    private bool MoveNextCellColor()
    {
        // Determine the color for the current cell
        var point = new Point2D(_x, _y);
        AnsiSequence targetColor;

        if (_nodeSet.Contains(point))
        {
            var isAlive = _generation[point];
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
        var point = new Point2D(_x, _y);
        char character;

        if (_nodeSet.Contains(point))
        {
            var isAlive = _generation[point];
            character = isAlive ? _theme.AliveChar : _theme.DeadChar;
        }
        else
        {
            character = ' ';
        }

        Current = Token.Char(character);
        _x++;

        if (_x > _renderEndX)
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
        var isAtRight = _viewport?.IsAtRight ?? true;
        var borderColor = (_viewport is not null && !isAtRight) ? AnsiSequence.ForegroundDarkGray : AnsiSequence.ForegroundGray;

        // Right border: color, vertical bar or right arrow
        if (_borderPosition == 0)
        {
            if (TryEmitColor(borderColor))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            Current = Token.Char(isAtRight ? ConsoleTheme.Border.Vertical : ConsoleTheme.ViewportBorder.Right);
            _borderPosition = 0;
            _phase = RenderPhase.RowNewline;
            return true;
        }

        return false;
    }

    private bool MoveNextRowNewline()
    {
        Current = WellKnownTokens.Newline;
        _y++;

        if (_y > _renderEndY)
        {
            _phase = _theme.ShowBorder ? RenderPhase.BottomBorder : RenderPhase.Done;
        }
        else
        {
            _x = _renderStartX;
            _phase = _theme.ShowBorder ? RenderPhase.LeftBorder : RenderPhase.CellColor;
        }

        return true;
    }

    private bool MoveNextBottomBorder()
    {
        // Bottom border: color, left corner, horizontals, right corner, newline
        var isAtBottom = _viewport?.IsAtBottom ?? true;
        var isAtLeft = _viewport?.IsAtLeft ?? true;
        var isAtRight = _viewport?.IsAtRight ?? true;
        var borderColor = (_viewport is not null && !isAtBottom) ? AnsiSequence.ForegroundDarkGray : AnsiSequence.ForegroundGray;

        if (_borderPosition == 0)
        {
            if (TryEmitColor(borderColor))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            Current = Token.Char(GetBottomLeftCorner(isAtBottom, isAtLeft));
            _borderPosition++;
            return true;
        }

        if (_borderPosition <= _width + 1)
        {
            Current = Token.Char(isAtBottom ? ConsoleTheme.Border.Horizontal : ConsoleTheme.ViewportBorder.Down);
            _borderPosition++;
            return true;
        }

        if (_borderPosition == _width + 2)
        {
            Current = Token.Char(GetBottomRightCorner(isAtBottom, isAtRight));
            _borderPosition++;
            return true;
        }

        if (_borderPosition == _width + 3)
        {
            Current = WellKnownTokens.Newline;
            _phase = RenderPhase.Done;
            return true;
        }

        return false;
    }

    private static char GetBottomLeftCorner(bool isAtBottom, bool isAtLeft)
    {
        if (isAtBottom && isAtLeft)
        {
            return ConsoleTheme.Border.BottomLeft;
        }

        if (isAtBottom)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        if (isAtLeft)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return ConsoleTheme.ViewportBorder.DiagonalBottomLeft;
    }

    private static char GetBottomRightCorner(bool isAtBottom, bool isAtRight)
    {
        if (isAtBottom && isAtRight)
        {
            return ConsoleTheme.Border.BottomRight;
        }

        if (isAtBottom)
        {
            return ConsoleTheme.Border.Horizontal;
        }

        if (isAtRight)
        {
            return ConsoleTheme.Border.Vertical;
        }

        return ConsoleTheme.ViewportBorder.DiagonalBottomRight;
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
