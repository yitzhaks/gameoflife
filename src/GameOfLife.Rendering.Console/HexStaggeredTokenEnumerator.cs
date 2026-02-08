using GameOfLife.Core;

namespace GameOfLife.Rendering.Console;

/// <summary>
/// A zero-allocation enumerator that yields tokens for hexagonal staggered console rendering.
/// </summary>
/// <remarks>
/// Staggered layout renders each hex cell as a single character with offset rows:
///       O   O   O        &lt;- r = -2
///     O   O   O   O      &lt;- r = -1
///   O   O   O   O   O    &lt;- r = 0 (center row)
///     O   O   O   O      &lt;- r = 1
///       O   O   O        &lt;- r = 2
/// </remarks>
public ref struct HexStaggeredTokenEnumerator
{
    private readonly IGeneration<HexPoint, bool> _generation;
    private readonly HexagonalTopology _topology;
    private readonly ConsoleTheme _theme;

    private int _currentRow;
    private int _currentCol;
    private RenderPhase _phase;
    private AnsiSequence? _lastColor;

    // Pre-computed row info
    private int _rowQMin;
    private int _rowQMax;
    private int _rowIndent;

    private enum RenderPhase
    {
        RowIndent,
        CellColor,
        CellChar,
        CellSpace,
        RowNewline,
        Done
    }

    /// <summary>
    /// Creates a new token enumerator for hexagonal staggered rendering.
    /// </summary>
    /// <param name="topology">The hexagonal topology.</param>
    /// <param name="layout">The layout providing positions.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <param name="theme">The rendering theme.</param>
    public HexStaggeredTokenEnumerator(
        HexagonalTopology topology,
        HexLayout layout,
        IGeneration<HexPoint, bool> generation,
        ConsoleTheme theme)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(generation);
        ArgumentNullException.ThrowIfNull(theme);

        _topology = topology;
        _generation = generation;
        _theme = theme;

        _currentRow = -topology.Radius;
        _phase = RenderPhase.RowIndent;
        _lastColor = null;
        Current = default;

        // Initialize first row info
        ComputeRowInfo(_currentRow);
        _currentCol = _rowQMin;
    }

    /// <summary>
    /// Gets the current token.
    /// </summary>
    public Token Current { get; private set; }

    /// <summary>
    /// Advances the enumerator to the next token.
    /// </summary>
    /// <returns>True if there is a next token; false if enumeration is complete.</returns>
    public bool MoveNext()
    {
        while (true)
        {
            switch (_phase)
            {
                case RenderPhase.RowIndent:
                    if (MoveNextRowIndent())
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

                case RenderPhase.CellSpace:
                    if (MoveNextCellSpace())
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

                case RenderPhase.Done:
                default:
                    return false;
            }
        }
    }

    private void ComputeRowInfo(int r)
    {
        int radius = _topology.Radius;
        _rowQMin = Math.Max(-radius, -r - radius);
        _rowQMax = Math.Min(radius, -r + radius);
        _rowIndent = Math.Abs(r);
    }

    private bool MoveNextRowIndent()
    {
        // Emit spaces for row indent
        if (_rowIndent > 0)
        {
            Current = WellKnownTokens.Space;
            _rowIndent--;
            return true;
        }

        _phase = RenderPhase.CellColor;
        return false;
    }

    private bool MoveNextCellColor()
    {
        HexPoint point = (_currentCol, _currentRow);
        bool isAlive = _generation[point];
        AnsiSequence targetColor = isAlive ? AnsiSequence.ForegroundGreen : AnsiSequence.ForegroundDarkGray;

        if (_lastColor != targetColor)
        {
            Current = Token.Ansi(targetColor);
            _lastColor = targetColor;
            _phase = RenderPhase.CellChar;
            return true;
        }

        _phase = RenderPhase.CellChar;
        return false;
    }

    private bool MoveNextCellChar()
    {
        HexPoint point = (_currentCol, _currentRow);
        bool isAlive = _generation[point];

        // Use hexagon characters for better visual representation
        char cellChar = isAlive ? _theme.AliveChar : _theme.DeadChar;
        Current = Token.Char(cellChar);

        _phase = RenderPhase.CellSpace;
        return true;
    }

    private bool MoveNextCellSpace()
    {
        _currentCol++;

        if (_currentCol <= _rowQMax)
        {
            // More cells in this row - emit space between cells
            Current = WellKnownTokens.Space;
            _phase = RenderPhase.CellColor;
            return true;
        }

        // End of row
        _phase = RenderPhase.RowNewline;
        return false;
    }

    private bool MoveNextRowNewline()
    {
        Current = WellKnownTokens.Newline;
        _currentRow++;

        if (_currentRow <= _topology.Radius)
        {
            // More rows to process
            ComputeRowInfo(_currentRow);
            _currentCol = _rowQMin;
            _phase = RenderPhase.RowIndent;
        }
        else
        {
            _phase = RenderPhase.Done;
        }

        return true;
    }

    /// <summary>
    /// Returns this enumerator (supports foreach).
    /// </summary>
    public readonly HexStaggeredTokenEnumerator GetEnumerator() => this;
}
