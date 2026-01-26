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
    private readonly int _width;

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
    public TokenEnumerator(
        ILayout<Point2D, Point2D, RectangularBounds> layout,
        IGeneration<Point2D, bool> generation,
        HashSet<Point2D> nodeSet,
        ConsoleTheme theme)
    {
        _generation = generation;
        _nodeSet = nodeSet;
        _theme = theme;
        _bounds = layout.Bounds;
        _width = _bounds.Max.X - _bounds.Min.X + 1;

        _x = _bounds.Min.X;
        _y = _bounds.Min.Y;
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
        if (_borderPosition == 0)
        {
            // Border color
            if (TryEmitColor(AnsiSequence.ForegroundGray))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            // Top left corner
            Current = Token.Char(ConsoleTheme.Border.TopLeft);
            _borderPosition++;
            return true;
        }

        if (_borderPosition <= _width + 1)
        {
            // Horizontal bars
            Current = Token.Char(ConsoleTheme.Border.Horizontal);
            _borderPosition++;
            return true;
        }

        if (_borderPosition == _width + 2)
        {
            // Top right corner
            Current = Token.Char(ConsoleTheme.Border.TopRight);
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

    private bool MoveNextLeftBorder()
    {
        if (!_theme.ShowBorder)
        {
            _phase = RenderPhase.CellColor;
            return false;
        }

        // Left border: color, vertical bar
        if (_borderPosition == 0)
        {
            if (TryEmitColor(AnsiSequence.ForegroundGray))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            Current = Token.Char(ConsoleTheme.Border.Vertical);
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

        if (_x > _bounds.Max.X)
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
        // Right border: color, vertical bar
        if (_borderPosition == 0)
        {
            if (TryEmitColor(AnsiSequence.ForegroundGray))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            Current = Token.Char(ConsoleTheme.Border.Vertical);
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

        if (_y > _bounds.Max.Y)
        {
            _phase = _theme.ShowBorder ? RenderPhase.BottomBorder : RenderPhase.Done;
        }
        else
        {
            _x = _bounds.Min.X;
            _phase = _theme.ShowBorder ? RenderPhase.LeftBorder : RenderPhase.CellColor;
        }

        return true;
    }

    private bool MoveNextBottomBorder()
    {
        // Bottom border: color, left corner, horizontals, right corner, newline
        if (_borderPosition == 0)
        {
            if (TryEmitColor(AnsiSequence.ForegroundGray))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            Current = Token.Char(ConsoleTheme.Border.BottomLeft);
            _borderPosition++;
            return true;
        }

        if (_borderPosition <= _width + 1)
        {
            Current = Token.Char(ConsoleTheme.Border.Horizontal);
            _borderPosition++;
            return true;
        }

        if (_borderPosition == _width + 2)
        {
            Current = Token.Char(ConsoleTheme.Border.BottomRight);
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
