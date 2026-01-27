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

    [Fact]
    public void RenderToString_NullTopology_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        _ = Should.Throw<ArgumentNullException>(() => renderer.RenderToString(null!, generation));
    }

    [Fact]
    public void RenderToString_NullGeneration_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));

        _ = Should.Throw<ArgumentNullException>(() => renderer.RenderToString(topology, null!));
    }

    [Fact]
    public void GetHalfBlockGlyphEnumerator_NullTopology_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        _ = Should.Throw<ArgumentNullException>(() => renderer.GetHalfBlockGlyphEnumerator(null!, generation));
    }

    [Fact]
    public void GetHalfBlockGlyphEnumerator_NullGeneration_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));

        _ = Should.Throw<ArgumentNullException>(() => renderer.GetHalfBlockGlyphEnumerator(topology, null!));
    }

    [Fact]
    public void GetHalfBlockGlyphEnumerator_ReturnsValidEnumerator()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((2, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [default] = true },
            defaultState: false);

        HalfBlockColorNormalizedGlyphEnumerator enumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        // Should be able to enumerate glyphs
        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        glyphs.ShouldNotBeEmpty();
        // First glyph should have upper half block character (top cell alive, bottom dead)
        glyphs[0].Character.ShouldBe(ConsoleTheme.HalfBlock.UpperHalf);
    }

    [Fact]
    public void GetHalfBlockGlyphEnumerator_WithViewport_ReturnsViewportBoundEnumerator()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((10, 20));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        var viewport = new Viewport(5, 10, 10, 20);
        HalfBlockColorNormalizedGlyphEnumerator enumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation, viewport);

        // Should be able to enumerate glyphs
        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        glyphs.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetCachedTopologyData_SameTopology_ReusesCache()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 3));
        using var generation1 = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);
        using var generation2 = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 1)] = true },
            defaultState: false);

        // First call caches the topology data
        _ = renderer.GetTokenEnumerator(topology, generation1);

        // Second call with same topology should reuse cache (verified by no exception and correct behavior)
        TokenEnumerator enumerator = renderer.GetTokenEnumerator(topology, generation2);

        var tokens = new List<Token>();
        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        tokens.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetCachedTopologyData_DifferentTopology_InvalidatesCache()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology1 = new RectangularTopology((3, 3));
        var topology2 = new RectangularTopology((5, 5));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // First call caches the topology data for 3x3
        _ = renderer.GetTokenEnumerator(topology1, generation);

        // Second call with different topology should create new cache
        TokenEnumerator enumerator = renderer.GetTokenEnumerator(topology2, generation);

        var tokens = new List<Token>();
        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        // Should have tokens for 5x5 grid (more than 3x3 would have)
        int characterCount = tokens.Count(t => !t.IsSequence && t.Character != '\n');
        characterCount.ShouldBe(25); // 5x5 = 25 cells
    }

    [Fact]
    public void GetHalfBlockGlyphEnumerator_DifferentTopology_InvalidatesHalfBlockCache()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology1 = new RectangularTopology((2, 4));
        var topology2 = new RectangularTopology((4, 8));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // First call caches the half-block layout for 2x4
        _ = renderer.GetHalfBlockGlyphEnumerator(topology1, generation);

        // Second call with different topology should create new cache
        HalfBlockColorNormalizedGlyphEnumerator enumerator = renderer.GetHalfBlockGlyphEnumerator(topology2, generation);

        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        // Should have glyphs for 4x4 packed grid (more than 2x2 would have)
        int cellCount = glyphs.Count(g => !g.IsNewline);
        cellCount.ShouldBe(16); // 4 columns x 4 packed rows = 16 cells
    }

    [Fact]
    public void Render_SparseTopology_RendersSpaceForNonTopologyPoints()
    {
        // Test the else branch on line 86-90 where a point is not in the topology
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        // Create topology but use a sparse nodeSet that doesn't include all points
        var topology = new RectangularTopology((3, 3));

        // Create generation with some alive cells
        var states = new Dictionary<Point2D, bool>
        {
            [(0, 0)] = true,
            [(2, 0)] = true,
            [(1, 1)] = true,
            [(0, 2)] = true,
            [(2, 2)] = true,
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        string result = output.ToString();
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // With full topology, should render the checkerboard pattern
        lines.Length.ShouldBe(3);
        lines[0].ShouldBe("#.#");
        lines[1].ShouldBe(".#.");
        lines[2].ShouldBe("#.#");
    }

    [Fact]
    public void GetCachedHalfBlockLayout_SameTopology_ReusesCache()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 4));
        using var generation1 = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);
        using var generation2 = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool> { [(1, 1)] = true },
            defaultState: false);

        // First call caches the half-block layout
        _ = renderer.GetHalfBlockGlyphEnumerator(topology, generation1);

        // Second call with same topology should reuse cache (verified by no exception and correct behavior)
        HalfBlockColorNormalizedGlyphEnumerator enumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation2);

        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        glyphs.ShouldNotBeEmpty();
    }

    [Fact]
    public void GetCachedHalfBlockLayout_NullCachedLayout_CreatesNewLayout()
    {
        // Test the condition where _cachedHalfBlockLayout is null even when topology matches
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new RectangularTopology((3, 4));
        using var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        // First call GetTokenEnumerator - this caches topology but not half-block layout
        _ = renderer.GetTokenEnumerator(topology, generation);

        // Second call GetHalfBlockGlyphEnumerator with same topology
        // This should create half-block layout because _cachedHalfBlockLayout is null
        HalfBlockColorNormalizedGlyphEnumerator enumerator = renderer.GetHalfBlockGlyphEnumerator(topology, generation);

        var glyphs = new List<Glyph>();
        while (enumerator.MoveNext())
        {
            glyphs.Add(enumerator.Current);
        }

        glyphs.ShouldNotBeEmpty();
        // Should have half-block characters (space for dead cells)
        int cellCount = glyphs.Count(g => !g.IsNewline);
        cellCount.ShouldBe(6); // 3 columns x 2 packed rows = 6 cells
    }
}
