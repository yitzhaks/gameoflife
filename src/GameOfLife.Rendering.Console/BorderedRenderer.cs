namespace GameOfLife.Rendering.Console;

/// <summary>
/// A ref struct that handles border token generation for console rendering.
/// </summary>
/// <remarks>
/// This struct wraps a <see cref="ViewportRenderer"/> and provides methods to generate
/// border tokens. It tracks the current position within each border and the last emitted
/// color to avoid redundant color changes. Designed to be embedded in token enumerators
/// for zero-allocation border rendering.
/// </remarks>
internal ref struct BorderedRenderer
{
    private readonly ViewportRenderer _viewportRenderer;
    private int _borderPosition;

    /// <summary>
    /// Creates a new bordered renderer wrapping the specified viewport renderer.
    /// </summary>
    /// <param name="viewportRenderer">The viewport renderer providing edge state and dimensions.</param>
    public BorderedRenderer(ViewportRenderer viewportRenderer)
    {
        _viewportRenderer = viewportRenderer;
        _borderPosition = 0;
        LastColor = null;
    }

    /// <summary>
    /// Gets the width of the rendered area (used for horizontal border calculations).
    /// </summary>
    public readonly int Width => _viewportRenderer.Width;

    /// <summary>
    /// Gets the last emitted color for tracking and handoff to cell rendering.
    /// </summary>
    public AnsiSequence? LastColor { get; private set; }

    /// <summary>
    /// Sets the last color, allowing the enumerator to synchronize color state.
    /// </summary>
    /// <param name="color">The color to set as last emitted.</param>
    public void SetLastColor(AnsiSequence? color) => LastColor = color;

    /// <summary>
    /// Attempts to get the next token for the top border.
    /// </summary>
    /// <param name="token">The output token if successful.</param>
    /// <returns>
    /// True if a token was produced; false if the top border is complete.
    /// When false is returned and the border is complete, the caller should transition
    /// to the left border phase.
    /// </returns>
    public bool TryGetTopBorderToken(out Token token)
    {
        bool isAtTop = _viewportRenderer.IsAtTop;
        bool isAtLeft = _viewportRenderer.IsAtLeft;
        bool isAtRight = _viewportRenderer.IsAtRight;
        AnsiSequence borderColor = BorderHelper.GetBorderColor(isAtTop);

        if (_borderPosition == 0)
        {
            // Border color
            if (TryEmitColor(borderColor, out token))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            // Top left corner
            token = Token.Char(BorderHelper.GetTopLeftCorner(isAtTop, isAtLeft));
            _borderPosition++;
            return true;
        }

        if (_borderPosition <= Width + 1)
        {
            // Horizontal bars or up arrows
            token = Token.Char(BorderHelper.GetHorizontalChar(isAtTop, isTop: true));
            _borderPosition++;
            return true;
        }

        if (_borderPosition == Width + 2)
        {
            // Top right corner
            token = Token.Char(BorderHelper.GetTopRightCorner(isAtTop, isAtRight));
            _borderPosition++;
            return true;
        }

        if (_borderPosition == Width + 3)
        {
            // Newline
            token = WellKnownTokens.Newline;
            _borderPosition = 0;
            return true;
        }

        token = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the next token for the left border of the current row.
    /// </summary>
    /// <param name="token">The output token if successful.</param>
    /// <returns>
    /// True if a token was produced; false if the left border is complete.
    /// When false is returned, the caller should transition to cell rendering.
    /// </returns>
    public bool TryGetLeftBorderToken(out Token token)
    {
        bool isAtLeft = _viewportRenderer.IsAtLeft;
        AnsiSequence borderColor = BorderHelper.GetBorderColor(isAtLeft);

        if (_borderPosition == 0)
        {
            if (TryEmitColor(borderColor, out token))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            token = Token.Char(BorderHelper.GetVerticalChar(isAtLeft, isLeft: true));
            _borderPosition = 0;
            return true;
        }

        token = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the next token for the right border of the current row.
    /// </summary>
    /// <param name="token">The output token if successful.</param>
    /// <returns>
    /// True if a token was produced; false if the right border is complete.
    /// When false is returned, the caller should transition to the row newline phase.
    /// </returns>
    public bool TryGetRightBorderToken(out Token token)
    {
        bool isAtRight = _viewportRenderer.IsAtRight;
        AnsiSequence borderColor = BorderHelper.GetBorderColor(isAtRight);

        if (_borderPosition == 0)
        {
            if (TryEmitColor(borderColor, out token))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            token = Token.Char(BorderHelper.GetVerticalChar(isAtRight, isLeft: false));
            _borderPosition = 0;
            return true;
        }

        token = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the next token for the bottom border.
    /// </summary>
    /// <param name="token">The output token if successful.</param>
    /// <param name="isComplete">
    /// Set to true when the bottom border (including newline) is complete.
    /// </param>
    /// <returns>
    /// True if a token was produced; false if the bottom border processing is complete.
    /// </returns>
    public bool TryGetBottomBorderToken(out Token token, out bool isComplete)
    {
        isComplete = false;
        bool isAtBottom = _viewportRenderer.IsAtBottom;
        bool isAtLeft = _viewportRenderer.IsAtLeft;
        bool isAtRight = _viewportRenderer.IsAtRight;
        AnsiSequence borderColor = BorderHelper.GetBorderColor(isAtBottom);

        if (_borderPosition == 0)
        {
            if (TryEmitColor(borderColor, out token))
            {
                _borderPosition++;
                return true;
            }

            _borderPosition++;
        }

        if (_borderPosition == 1)
        {
            // Bottom left corner
            token = Token.Char(BorderHelper.GetBottomLeftCorner(isAtBottom, isAtLeft));
            _borderPosition++;
            return true;
        }

        if (_borderPosition <= Width + 1)
        {
            // Horizontal bars or down arrows
            token = Token.Char(BorderHelper.GetHorizontalChar(isAtBottom, isTop: false));
            _borderPosition++;
            return true;
        }

        if (_borderPosition == Width + 2)
        {
            // Bottom right corner
            token = Token.Char(BorderHelper.GetBottomRightCorner(isAtBottom, isAtRight));
            _borderPosition++;
            return true;
        }

        if (_borderPosition == Width + 3)
        {
            // Newline
            token = WellKnownTokens.Newline;
            isComplete = true;
            return true;
        }

        token = default;
        return false;
    }

    private bool TryEmitColor(AnsiSequence color, out Token token)
    {
        if (LastColor != color)
        {
            token = Token.Ansi(color);
            LastColor = color;
            return true;
        }

        token = default;
        return false;
    }
}
