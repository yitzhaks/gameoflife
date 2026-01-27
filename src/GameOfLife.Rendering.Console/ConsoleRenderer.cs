using System.Text;

using GameOfLife.Core;

namespace GameOfLife.Rendering.Console;

/// <summary>
/// Renders Game of Life generations to the console.
/// </summary>
public sealed class ConsoleRenderer : IRenderer<RectangularTopology, Point2D, Point2D, RectangularBounds, bool>
{
    private readonly TextWriter _output;
    private readonly ILayoutEngine<RectangularTopology, Point2D, Point2D, RectangularBounds> _layoutEngine;
    private readonly ConsoleTheme _theme;

    // Cached topology data to avoid re-creating per frame
    private RectangularTopology? _cachedTopology;
    private HashSet<Point2D>? _cachedNodeSet;
    private ILayout<Point2D, Point2D, RectangularBounds>? _cachedLayout;

    /// <summary>
    /// Creates a new console renderer.
    /// </summary>
    /// <param name="output">The text writer to output to.</param>
    /// <param name="layoutEngine">The layout engine to use for computing positions.</param>
    /// <param name="theme">The theme for rendering appearance.</param>
    /// <exception cref="ArgumentNullException">Thrown if output, layoutEngine, or theme is null.</exception>
    public ConsoleRenderer(
        TextWriter output,
        ILayoutEngine<RectangularTopology, Point2D, Point2D, RectangularBounds> layoutEngine,
        ConsoleTheme theme)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(layoutEngine);
        ArgumentNullException.ThrowIfNull(theme);

        _output = output;
        _layoutEngine = layoutEngine;
        _theme = theme;
    }

    /// <summary>
    /// Renders the generation state for the specified topology without colors.
    /// For colored output, use <see cref="GetTokenEnumerator"/> or <see cref="RenderToString"/> which produce ANSI sequences.
    /// Iterates Y then X (top to bottom, left to right).
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public void Render(RectangularTopology topology, IGeneration<Point2D, bool> generation)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(generation);

        (ILayout<Point2D, Point2D, RectangularBounds> layout, HashSet<Point2D> nodeSet) = GetCachedTopologyData(topology);
        RectangularBounds bounds = layout.Bounds;
        int width = bounds.Max.X - bounds.Min.X + 1;

        // Draw top border
        if (_theme.ShowBorder)
        {
            DrawBorder(ConsoleTheme.Border.TopLeft, ConsoleTheme.Border.Horizontal, ConsoleTheme.Border.TopRight, width);
        }

        // Iterate Y then X (top to bottom, left to right)
        for (int y = bounds.Min.Y; y <= bounds.Max.Y; y++)
        {
            var rowBuilder = new StringBuilder();

            // Left border
            if (_theme.ShowBorder)
            {
                _ = rowBuilder.Append(ConsoleTheme.Border.Vertical);
            }

            for (int x = bounds.Min.X; x <= bounds.Max.X; x++)
            {
                Point2D point = (x, y);
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

            // Right border
            if (_theme.ShowBorder)
            {
                _ = rowBuilder.Append(ConsoleTheme.Border.Vertical);
            }

            _output.WriteLine(rowBuilder);
        }

        // Draw bottom border
        if (_theme.ShowBorder)
        {
            DrawBorder(ConsoleTheme.Border.BottomLeft, ConsoleTheme.Border.Horizontal, ConsoleTheme.Border.BottomRight, width);
        }
    }

    private void DrawBorder(char left, char middle, char right, int width)
    {
        _output.Write(left);
        _output.Write(new string(middle, width));
        _output.Write(right);
        _output.WriteLine();
    }

    /// <summary>
    /// Gets a token enumerator for streaming rendering.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <param name="viewport">Optional viewport for clipping large boards.</param>
    /// <returns>A token enumerator that yields rendering tokens.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public TokenEnumerator GetTokenEnumerator(RectangularTopology topology, IGeneration<Point2D, bool> generation, Viewport? viewport = null)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(generation);

        (ILayout<Point2D, Point2D, RectangularBounds> layout, HashSet<Point2D> nodeSet) = GetCachedTopologyData(topology);

        return new TokenEnumerator(layout, generation, nodeSet, _theme, viewport);
    }

    private (ILayout<Point2D, Point2D, RectangularBounds> Layout, HashSet<Point2D> NodeSet) GetCachedTopologyData(RectangularTopology topology)
    {
        // Cache layout and node set for the topology (topologies are immutable and typically reused)
        if (_cachedTopology != topology)
        {
            _cachedTopology = topology;
            _cachedLayout = _layoutEngine.CreateLayout(topology);
#pragma warning disable IDE0028, IDE0306 // Collection initialization can be simplified - HashSet requires constructor
            _cachedNodeSet = new HashSet<Point2D>(topology.Nodes);
#pragma warning restore IDE0028, IDE0306
        }

        return (_cachedLayout!, _cachedNodeSet!);
    }

    /// <summary>
    /// Gets a color-normalized glyph enumerator for streaming rendering.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <param name="viewport">Optional viewport for clipping large boards.</param>
    /// <returns>A color-normalized glyph enumerator.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public ColorNormalizedGlyphEnumerator GetGlyphEnumerator(RectangularTopology topology, IGeneration<Point2D, bool> generation, Viewport? viewport = null)
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
    public string RenderToString(RectangularTopology topology, IGeneration<Point2D, bool> generation)
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
