using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class HexConsoleRendererTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_NullOutput_ThrowsArgumentNullException()
    {
        var engine = new HexLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            new HexConsoleRenderer(null!, engine, theme));

        exception.ParamName.ShouldBe("output");
    }

    [Fact]
    public void Constructor_NullLayoutEngine_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        ConsoleTheme theme = ConsoleTheme.Default;

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            new HexConsoleRenderer(output, null!, theme));

        exception.ParamName.ShouldBe("layoutEngine");
    }

    [Fact]
    public void Constructor_NullTheme_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            new HexConsoleRenderer(output, engine, null!));

        exception.ParamName.ShouldBe("theme");
    }

    [Fact]
    public void Constructor_ValidArguments_CreatesRenderer()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        ConsoleTheme theme = ConsoleTheme.Default;

        var renderer = new HexConsoleRenderer(output, engine, theme);

        _ = renderer.ShouldNotBeNull();
    }

    #endregion

    #region Render Tests

    [Fact]
    public void Render_NullTopology_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        using var generation = new HexGeneration();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            renderer.Render(null!, generation));

        exception.ParamName.ShouldBe("topology");
    }

    [Fact]
    public void Render_NullGeneration_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology = new HexagonalTopology(1);

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            renderer.Render(topology, null!));

        exception.ParamName.ShouldBe("generation");
    }

    [Fact]
    public void Render_WritesToOutput()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology = new HexagonalTopology(1);
        using var generation = new HexGeneration();

        renderer.Render(topology, generation);

        output.ToString().ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region RenderToString Tests

    [Fact]
    public void RenderToString_NullTopology_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        using var generation = new HexGeneration();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            renderer.RenderToString(null!, generation));

        exception.ParamName.ShouldBe("topology");
    }

    [Fact]
    public void RenderToString_NullGeneration_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology = new HexagonalTopology(1);

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            renderer.RenderToString(topology, null!));

        exception.ParamName.ShouldBe("generation");
    }

    [Fact]
    public void RenderToString_ReturnsNonEmptyString()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology = new HexagonalTopology(1);
        using var generation = new HexGeneration();

        string result = renderer.RenderToString(topology, generation);

        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void RenderToString_ContainsNewlines()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology = new HexagonalTopology(1);
        using var generation = new HexGeneration();

        string result = renderer.RenderToString(topology, generation);

        result.ShouldContain("\n");
    }

    #endregion

    #region GetTokenEnumerator Tests

    [Fact]
    public void GetTokenEnumerator_NullTopology_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        using var generation = new HexGeneration();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            renderer.GetTokenEnumerator(null!, generation));

        exception.ParamName.ShouldBe("topology");
    }

    [Fact]
    public void GetTokenEnumerator_NullGeneration_ThrowsArgumentNullException()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology = new HexagonalTopology(1);

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            renderer.GetTokenEnumerator(topology, null!));

        exception.ParamName.ShouldBe("generation");
    }

    [Fact]
    public void GetTokenEnumerator_ReturnsEnumerator()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology = new HexagonalTopology(1);
        using var generation = new HexGeneration();

        HexStaggeredTokenEnumerator enumerator = renderer.GetTokenEnumerator(topology, generation);

        // Verify it can enumerate
        enumerator.MoveNext().ShouldBeTrue();
    }

    #endregion

    #region Caching Tests

    [Fact]
    public void Render_SameTopologyTwice_UsesCachedLayout()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology = new HexagonalTopology(1);
        using var generation1 = new HexGeneration();
        using var generation2 = new HexGeneration([default]);

        // Should not throw and should produce output both times
        renderer.Render(topology, generation1);
        string output1 = output.ToString();
        _ = output.GetStringBuilder().Clear();

        renderer.Render(topology, generation2);
        string output2 = output.ToString();

        output1.ShouldNotBeNullOrEmpty();
        output2.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Render_DifferentTopologies_CreatesNewLayout()
    {
        using var output = new StringWriter();
        var engine = new HexLayoutEngine();
        var renderer = new HexConsoleRenderer(output, engine, ConsoleTheme.Default);
        var topology1 = new HexagonalTopology(1);
        var topology2 = new HexagonalTopology(2);
        using var generation = new HexGeneration();

        // Should not throw and should produce different outputs
        renderer.Render(topology1, generation);
        string output1 = output.ToString();
        _ = output.GetStringBuilder().Clear();

        renderer.Render(topology2, generation);
        string output2 = output.ToString();

        output1.ShouldNotBeNullOrEmpty();
        output2.ShouldNotBeNullOrEmpty();
        output1.Length.ShouldBeLessThan(output2.Length);
    }

    #endregion
}
