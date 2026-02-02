using System.Text;

using GameOfLife.Core;

namespace GameOfLife.Rendering.Console;

/// <summary>
/// Renders hexagonal Game of Life generations to the console using staggered layout.
/// </summary>
public sealed class HexConsoleRenderer : IRenderer<HexagonalTopology, HexPoint, Point2D, HexBounds, bool>
{
    private readonly TextWriter _output;
    private readonly HexLayoutEngine _layoutEngine;
    private readonly ConsoleTheme _theme;

    // Cached topology data to avoid re-creating per frame
    private HexagonalTopology? _cachedTopology;
    private HexLayout? _cachedLayout;

    /// <summary>
    /// Creates a new hexagonal console renderer.
    /// </summary>
    /// <param name="output">The text writer to output to.</param>
    /// <param name="layoutEngine">The layout engine to use for computing positions.</param>
    /// <param name="theme">The theme for rendering appearance.</param>
    /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
    public HexConsoleRenderer(
        TextWriter output,
        HexLayoutEngine layoutEngine,
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
    /// Renders the generation state for the specified topology.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public void Render(HexagonalTopology topology, IGeneration<HexPoint, bool> generation)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(generation);

        string output = RenderToString(topology, generation);
        _output.Write(output);
    }

    /// <summary>
    /// Gets a token enumerator for streaming rendering.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <returns>A staggered token enumerator.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public HexStaggeredTokenEnumerator GetTokenEnumerator(
        HexagonalTopology topology,
        IGeneration<HexPoint, bool> generation)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(generation);

        HexLayout layout = GetCachedLayout(topology);
        return new HexStaggeredTokenEnumerator(topology, layout, generation, _theme);
    }

    /// <summary>
    /// Renders the generation state to a string with ANSI color codes.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    /// <returns>A string containing the rendered output with ANSI codes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if topology or generation is null.</exception>
    public string RenderToString(HexagonalTopology topology, IGeneration<HexPoint, bool> generation)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(generation);

        var sb = new StringBuilder();

        HexStaggeredTokenEnumerator enumerator = GetTokenEnumerator(topology, generation);
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
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

    private HexLayout GetCachedLayout(HexagonalTopology topology)
    {
        if (_cachedTopology != topology)
        {
            _cachedTopology = topology;
            _cachedLayout = (HexLayout)_layoutEngine.CreateLayout(topology);
        }

        return _cachedLayout!;
    }
}
