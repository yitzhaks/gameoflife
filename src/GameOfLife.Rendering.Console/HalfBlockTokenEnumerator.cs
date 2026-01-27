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
    private readonly PackedBounds _bounds;
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
        RightBorder,
        RowNewline,
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
        _bounds = layout.Bounds;
        _viewport = viewport;

        // Calculate render bounds based on viewport (in packed space)
        if (viewport is not null)
        {
            _renderStartX = viewport.OffsetX;
            _renderStartY = viewport.OffsetY;
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
            _width = _bounds.Width;
        }

        _x = _renderStartX;
        _y = _renderStartY;
        _phase = theme.ShowBorder ? RenderPhase.TopBorder : RenderPhase.CellForegroundColor;
        _borderPosition = 0;
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

                default:
                    throw new InvalidOperationException($"Unhandled render phase: {_phase}");
            }
        }
    }

    private bool MoveNextTopBorder()
    {
        // Top border: color, left corner, horizontals, right corner, newline
        bool isAtTop = _viewport?.IsAtTop ?? true;
        bool isAtLeft = _viewport?.IsAtLeft ?? true;
        bool isAtRight = _viewport?.IsAtRight ?? true;
        AnsiSequence borderColor = (_viewport is not null && !isAtTop) ? AnsiSequence.ForegroundDarkGray : AnsiSequence.ForegroundGray;

        if (_borderPosition == 0)
        {
            // Border color
            if (TryEmitForegroundColor(borderColor))
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
            _phase = RenderPhase.CellForegroundColor;
            return false;
        }

        bool isAtLeft = _viewport?.IsAtLeft ?? true;
        AnsiSequence borderColor = (_viewport is not null && !isAtLeft) ? AnsiSequence.ForegroundDarkGray : AnsiSequence.ForegroundGray;

        // Left border: color, vertical bar or left arrow
        if (_borderPosition == 0)
        {
            if (TryEmitForegroundColor(borderColor))
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
            _phase = RenderPhase.CellForegroundColor;
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
            // Points not in topology - emit space with no color change
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

        if (_x > _renderEndX)
        {
            _phase = _theme.ShowBorder ? RenderPhase.RightBorder : RenderPhase.RowNewline;
        }
        else
        {
            _phase = RenderPhase.CellForegroundColor;
        }

        return true;
    }

    private bool MoveNextRightBorder()
    {
        bool isAtRight = _viewport?.IsAtRight ?? true;
        AnsiSequence borderColor = (_viewport is not null && !isAtRight) ? AnsiSequence.ForegroundDarkGray : AnsiSequence.ForegroundGray;

        // Right border: color, vertical bar or right arrow
        if (_borderPosition == 0)
        {
            if (TryEmitForegroundColor(borderColor))
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
            _phase = _theme.ShowBorder ? RenderPhase.LeftBorder : RenderPhase.CellForegroundColor;
        }

        return true;
    }

    private bool MoveNextBottomBorder()
    {
        // Bottom border: color, left corner, horizontals, right corner, newline
        bool isAtBottom = _viewport?.IsAtBottom ?? true;
        bool isAtLeft = _viewport?.IsAtLeft ?? true;
        bool isAtRight = _viewport?.IsAtRight ?? true;
        AnsiSequence borderColor = (_viewport is not null && !isAtBottom) ? AnsiSequence.ForegroundDarkGray : AnsiSequence.ForegroundGray;

        if (_borderPosition == 0)
        {
            if (TryEmitForegroundColor(borderColor))
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
