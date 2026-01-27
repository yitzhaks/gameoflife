using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ConsoleRendererTests
{
    [Fact]
    public void Render_3x3GridAllDead_RendersAllDeadCharacters()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        renderer.Render(topology, generation);

        string result = output.ToString();
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(3);
        lines.ShouldAllBe(line => line == "...");
    }

    [Fact]
    public void Render_3x3GridAllAlive_RendersAllAliveCharacters()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));
        var states = new Dictionary<Point2D, bool>();
        foreach (Point2D node in topology.Nodes)
        {
            states[node] = true;
        }

        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        string result = output.ToString();
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(3);
        lines.ShouldAllBe(line => line == "###");
    }

    [Fact]
    public void Render_GliderPattern_RendersCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));
        // Glider pattern:
        // .#.
        // ..#
        // ###
        var states = new Dictionary<Point2D, bool>
        {
            [(1, 0)] = true,
            [(2, 1)] = true,
            [(0, 2)] = true,
            [(1, 2)] = true,
            [(2, 2)] = true
        };

        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        string result = output.ToString();
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(3);
        lines[0].ShouldBe(".#.");
        lines[1].ShouldBe("..#");
        lines[2].ShouldBe("###");
    }

    [Fact]
    public void Render_CustomTheme_UsesCustomCharacters()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: 'O', DeadChar: '-', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 1)] = true
        };

        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        string result = output.ToString();
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(2);
        lines[0].ShouldBe("O-");
        lines[1].ShouldBe("-O");
    }

    [Fact]
    public void Render_WithBorder_RendersBorderAroundGrid()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 2));
        var states = new Dictionary<Point2D, bool>
        {
            [(1, 0)] = true,
            [(1, 1)] = true
        };

        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        string result = output.ToString();
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Expected output with double-line border:
        // ╔═══╗
        // ║.#.║
        // ║.#.║
        // ╚═══╝
        lines.Length.ShouldBe(4);
        lines[0].ShouldBe("╔═══╗");
        lines[1].ShouldBe("║.#.║");
        lines[2].ShouldBe("║.#.║");
        lines[3].ShouldBe("╚═══╝");
    }

    [Fact]
    public void Constructor_NullOutput_ThrowsArgumentNullException()
    {
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;

        _ = Should.Throw<ArgumentNullException>(() => new ConsoleRenderer(null!, engine, theme));
    }

    [Fact]
    public void Constructor_NullLayoutEngine_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        ConsoleTheme theme = ConsoleTheme.Default;

        _ = Should.Throw<ArgumentNullException>(() => new ConsoleRenderer(output, null!, theme));
    }

    [Fact]
    public void Constructor_NullTheme_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();

        _ = Should.Throw<ArgumentNullException>(() => new ConsoleRenderer(output, engine, null!));
    }

    [Fact]
    public void Render_NullTopology_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        _ = Should.Throw<ArgumentNullException>(() => renderer.Render(null!, generation));
    }

    [Fact]
    public void Render_NullGeneration_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));

        _ = Should.Throw<ArgumentNullException>(() => renderer.Render(topology, null!));
    }

    [Fact]
    public void GetTokenEnumerator_WithoutViewport_ReturnsValidEnumerator()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        TokenEnumerator enumerator = renderer.GetTokenEnumerator(topology, generation);

        // Should be able to enumerate tokens
        var tokens = new List<Token>();
        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        tokens.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetTokenEnumerator_WithViewport_ReturnsViewportBoundEnumerator()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((10, 10));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var viewport = new Viewport(3, 3, 10, 10);
        TokenEnumerator enumerator = renderer.GetTokenEnumerator(topology, generation, viewport);

        // Should be able to enumerate tokens
        var tokens = new List<Token>();
        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        tokens.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetGlyphEnumerator_ReturnsColorNormalizedEnumerator()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        ColorNormalizedGlyphEnumerator enumerator = renderer.GetGlyphEnumerator(topology, generation);

        // Should be able to enumerate glyphs
        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        glyphs.ShouldNotBeEmpty();
        // First glyph should have a color (alive cell)
        glyphs[0].Color.HasValue.ShouldBeTrue();
    }

    [Fact]
    public void GetGlyphEnumerator_WithViewport_ReturnsViewportBoundEnumerator()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((10, 10));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var viewport = new Viewport(3, 3, 10, 10);
        ColorNormalizedGlyphEnumerator enumerator = renderer.GetGlyphEnumerator(topology, generation, viewport);

        // Should be able to enumerate glyphs
        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        glyphs.ShouldNotBeEmpty();
    }

    [Fact]
    public void RenderToString_ProducesAnsiOutput()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        string result = renderer.RenderToString(topology, generation);

        // Should contain ANSI escape sequences
        result.ShouldContain("\x1b[");
        // Should contain the alive character
        result.ShouldContain("#");
        // Should contain the dead character
        result.ShouldContain(".");
    }

    [Fact]
    public void RenderToString_WithBorders_IncludesBorderCharacters()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 2));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        string result = renderer.RenderToString(topology, generation);

        // Should contain border characters
        result.ShouldContain("╔");
        result.ShouldContain("╗");
        result.ShouldContain("╚");
        result.ShouldContain("╝");
        result.ShouldContain("═");
        result.ShouldContain("║");
    }

    [Fact]
    public void GetTokenEnumerator_NullTopology_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        _ = Should.Throw<ArgumentNullException>(() => renderer.GetTokenEnumerator(null!, generation));
    }

    [Fact]
    public void GetTokenEnumerator_NullGeneration_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));

        _ = Should.Throw<ArgumentNullException>(() => renderer.GetTokenEnumerator(topology, null!));
    }
}
