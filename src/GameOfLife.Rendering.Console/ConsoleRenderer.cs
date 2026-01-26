using System.Text;

using GameOfLife.Core;

namespace GameOfLife.Rendering.Console;

/// <summary>
/// Renders Game of Life generations to the console.
/// </summary>
public sealed class ConsoleRenderer : IRenderer<Grid2DTopology, Point2D, Point2D, RectangularBounds, bool>
{
    private readonly TextWriter _output;
    private readonly ILayoutEngine<Grid2DTopology, Point2D, Point2D, RectangularBounds> _layoutEngine;
    private readonly ConsoleTheme _theme;
    private readonly bool _supportsColor;

    /// <summary>
    /// Creates a new console renderer.
    /// </summary>
    /// <param name="output">The text writer to output to.</param>
    /// <param name="layoutEngine">The layout engine to use for computing positions.</param>
    /// <param name="theme">The theme for rendering appearance.</param>
    /// <exception cref="ArgumentNullException">Thrown if output, layoutEngine, or theme is null.</exception>
    public ConsoleRenderer(
        TextWriter output,
        ILayoutEngine<Grid2DTopology, Point2D, Point2D, RectangularBounds> layoutEngine,
        ConsoleTheme theme)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(layoutEngine);
        ArgumentNullException.ThrowIfNull(theme);

        _output = output;
        _layoutEngine = layoutEngine;
        _theme = theme;
        _supportsColor = ReferenceEquals(output, System.Console.Out);
    }

    /// <summary>
    /// Renders the generation state for the specified topology.
    /// Iterates Y then X (top to bottom, left to right).
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public void Render(Grid2DTopology topology, IGeneration<Point2D, bool> generation)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(generation);

        ILayout<Point2D, Point2D, RectangularBounds> layout = _layoutEngine.CreateLayout(topology);
        RectangularBounds bounds = layout.Bounds;

        // Build a set of valid nodes for quick lookup
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        // Store the original color to restore later
        ConsoleColor? originalColor = null;
        if (_supportsColor)
        {
            originalColor = System.Console.ForegroundColor;
        }

        try
        {
            int width = bounds.Max.X - bounds.Min.X + 1;

            // Draw top border
            if (_theme.ShowBorder)
            {
                DrawBorder(ConsoleTheme.Border.TopLeft, ConsoleTheme.Border.Horizontal, ConsoleTheme.Border.TopRight, width);
            }

            // Iterate Y then X (top to bottom, left to right)
            for (int y = bounds.Min.Y; y <= bounds.Max.Y; y++)
            {
                // Left border
                if (_theme.ShowBorder)
                {
                    SetColor(_theme.BorderColor);
                    _output.Write(ConsoleTheme.Border.Vertical);
                }

                if (_supportsColor)
                {
                    // When using colors, we need to write character by character
                    for (int x = bounds.Min.X; x <= bounds.Max.X; x++)
                    {
                        var point = new Point2D(x, y);
                        if (nodeSet.Contains(point))
                        {
                            bool isAlive = generation[point];
                            System.Console.ForegroundColor = isAlive ? _theme.AliveColor : _theme.DeadColor;
                            _output.Write(isAlive ? _theme.AliveChar : _theme.DeadChar);
                        }
                        else
                        {
                            // For points not in the topology, write a space
                            _output.Write(' ');
                        }
                    }
                }
                else
                {
                    // When not using colors, we can build the row as a string
                    var rowBuilder = new StringBuilder();
                    for (int x = bounds.Min.X; x <= bounds.Max.X; x++)
                    {
                        var point = new Point2D(x, y);
                        if (nodeSet.Contains(point))
                        {
                            bool isAlive = generation[point];
                            _ = rowBuilder.Append(isAlive ? _theme.AliveChar : _theme.DeadChar);
                        }
                        else
                        {
                            // For points not in the topology, write a space
                            _ = rowBuilder.Append(' ');
                        }
                    }

                    _output.Write(rowBuilder);
                }

                // Right border
                if (_theme.ShowBorder)
                {
                    SetColor(_theme.BorderColor);
                    _output.Write(ConsoleTheme.Border.Vertical);
                }

                _output.WriteLine();
            }

            // Draw bottom border
            if (_theme.ShowBorder)
            {
                DrawBorder(ConsoleTheme.Border.BottomLeft, ConsoleTheme.Border.Horizontal, ConsoleTheme.Border.BottomRight, width);
            }
        }
        finally
        {
            // Restore the original color
            if (_supportsColor && originalColor.HasValue)
            {
                System.Console.ForegroundColor = originalColor.Value;
            }
        }
    }

    private void DrawBorder(char left, char middle, char right, int width)
    {
        SetColor(_theme.BorderColor);
        _output.Write(left);
        _output.Write(new string(middle, width));
        _output.Write(right);
        _output.WriteLine();
    }

    private void SetColor(ConsoleColor color)
    {
        if (_supportsColor)
        {
            System.Console.ForegroundColor = color;
        }
    }

    /// <summary>
    /// Gets a token enumerator for streaming rendering.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <param name="viewport">Optional viewport for clipping large boards.</param>
    /// <returns>A token enumerator that yields rendering tokens.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public TokenEnumerator GetTokenEnumerator(Grid2DTopology topology, IGeneration<Point2D, bool> generation, Viewport? viewport = null)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(generation);

        ILayout<Point2D, Point2D, RectangularBounds> layout = _layoutEngine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        return new TokenEnumerator(layout, generation, nodeSet, _theme, viewport);
    }

    /// <summary>
    /// Gets a color-normalized glyph enumerator for streaming rendering.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <param name="viewport">Optional viewport for clipping large boards.</param>
    /// <returns>A color-normalized glyph enumerator.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public ColorNormalizedGlyphEnumerator GetGlyphEnumerator(Grid2DTopology topology, IGeneration<Point2D, bool> generation, Viewport? viewport = null)
    {
        TokenEnumerator tokenEnumerator = GetTokenEnumerator(topology, generation, viewport);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);
        return AnsiStateTracker.FromGlyphs(glyphEnumerator);
    }

    /// <summary>
    /// Renders the generation state to a string with ANSI color codes.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <returns>A string containing the rendered output with ANSI codes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public string RenderToString(Grid2DTopology topology, IGeneration<Point2D, bool> generation)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(generation);

        var sb = new StringBuilder();
        TokenEnumerator tokenEnumerator = GetTokenEnumerator(topology, generation);

        while (tokenEnumerator.MoveNext())
        {
            Token token = tokenEnumerator.Current;
            if (token.IsSequence)
            {
                _ = sb.Append(token.Sequence.ToAnsiString());
            }
            else
            {
                _ = sb.Append(token.Character);
            }
        }

        return sb.ToString();
    }
}
