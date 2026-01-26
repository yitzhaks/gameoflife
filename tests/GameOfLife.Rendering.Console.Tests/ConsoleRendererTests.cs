using GameOfLife.Core;
using GameOfLife.Rendering;

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

        var topology = new Grid2DTopology(3, 3);
        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        renderer.Render(topology, generation);

        var result = output.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);
        Assert.All(lines, line => Assert.Equal("...", line));
    }

    [Fact]
    public void Render_3x3GridAllAlive_RendersAllAliveCharacters()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 3);
        var states = new Dictionary<Point2D, bool>();
        foreach (var node in topology.Nodes)
        {
            states[node] = true;
        }

        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        var result = output.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);
        Assert.All(lines, line => Assert.Equal("###", line));
    }

    [Fact]
    public void Render_GliderPattern_RendersCorrectly()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 3);
        // Glider pattern:
        // .#.
        // ..#
        // ###
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(1, 0)] = true,
            [new Point2D(2, 1)] = true,
            [new Point2D(0, 2)] = true,
            [new Point2D(1, 2)] = true,
            [new Point2D(2, 2)] = true
        };

        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        var result = output.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);
        Assert.Equal(".#.", lines[0]);
        Assert.Equal("..#", lines[1]);
        Assert.Equal("###", lines[2]);
    }

    [Fact]
    public void Render_CustomTheme_UsesCustomCharacters()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: 'O', DeadChar: '-', ShowBorder: false);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(2, 2);
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(0, 0)] = true,
            [new Point2D(1, 1)] = true
        };

        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        var result = output.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.Equal("O-", lines[0]);
        Assert.Equal("-O", lines[1]);
    }

    [Fact]
    public void Render_WithBorder_RendersBorderAroundGrid()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 2);
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(1, 0)] = true,
            [new Point2D(1, 1)] = true
        };

        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        renderer.Render(topology, generation);

        var result = output.ToString();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Expected output with double-line border:
        // ╔═══╗
        // ║.#.║
        // ║.#.║
        // ╚═══╝
        Assert.Equal(4, lines.Length);
        Assert.Equal("╔═══╗", lines[0]);
        Assert.Equal("║.#.║", lines[1]);
        Assert.Equal("║.#.║", lines[2]);
        Assert.Equal("╚═══╝", lines[3]);
    }

    [Fact]
    public void Constructor_NullOutput_ThrowsArgumentNullException()
    {
        var engine = new IdentityLayoutEngine();
        var theme = ConsoleTheme.Default;

        Assert.Throws<ArgumentNullException>(() => new ConsoleRenderer(null!, engine, theme));
    }

    [Fact]
    public void Constructor_NullLayoutEngine_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var theme = ConsoleTheme.Default;

        Assert.Throws<ArgumentNullException>(() => new ConsoleRenderer(output, null!, theme));
    }

    [Fact]
    public void Constructor_NullTheme_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();

        Assert.Throws<ArgumentNullException>(() => new ConsoleRenderer(output, engine, null!));
    }

    [Fact]
    public void Render_NullTopology_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        var generation = new DictionaryGeneration<Point2D, bool>(
            new Dictionary<Point2D, bool>(),
            defaultState: false);

        Assert.Throws<ArgumentNullException>(() => renderer.Render(null!, generation));
    }

    [Fact]
    public void Render_NullGeneration_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new IdentityLayoutEngine();
        var theme = ConsoleTheme.Default;
        var renderer = new ConsoleRenderer(output, engine, theme);

        var topology = new Grid2DTopology(3, 3);

        Assert.Throws<ArgumentNullException>(() => renderer.Render(topology, null!));
    }
}
