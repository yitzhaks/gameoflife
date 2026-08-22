using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class HexStaggeredTokenEnumeratorTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_NullTopology_ThrowsArgumentNullException()
    {
        var layout = new HexLayout(new HexagonalTopology(1));
        using var generation = new HexGeneration();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            _ = new HexStaggeredTokenEnumerator(null!, layout, generation, ConsoleTheme.Default));

        exception.ParamName.ShouldBe("topology");
    }

    [Fact]
    public void Constructor_NullLayout_ThrowsArgumentNullException()
    {
        var topology = new HexagonalTopology(1);
        using var generation = new HexGeneration();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            _ = new HexStaggeredTokenEnumerator(topology, null!, generation, ConsoleTheme.Default));

        exception.ParamName.ShouldBe("layout");
    }

    [Fact]
    public void Constructor_NullGeneration_ThrowsArgumentNullException()
    {
        var topology = new HexagonalTopology(1);
        var layout = new HexLayout(topology);

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            _ = new HexStaggeredTokenEnumerator(topology, layout, null!, ConsoleTheme.Default));

        exception.ParamName.ShouldBe("generation");
    }

    [Fact]
    public void Constructor_NullTheme_ThrowsArgumentNullException()
    {
        var topology = new HexagonalTopology(1);
        var layout = new HexLayout(topology);
        using var generation = new HexGeneration();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            _ = new HexStaggeredTokenEnumerator(topology, layout, generation, null!));

        exception.ParamName.ShouldBe("theme");
    }

    #endregion

    #region Token Enumeration Tests

    [Fact]
    public void MoveNext_RadiusZero_ProducesTokens()
    {
        var topology = new HexagonalTopology(0);
        var layout = new HexLayout(topology);
        using var generation = new HexGeneration();

        var enumerator = new HexStaggeredTokenEnumerator(topology, layout, generation, ConsoleTheme.Default);
        var tokens = new List<Token>();

        while (enumerator.MoveNext())
        {
            tokens.Add(enumerator.Current);
        }

        // Should have at least one character token and one newline
        tokens.Count.ShouldBeGreaterThan(0);
        tokens.ShouldContain(t => t.IsCharacter && t.Character == '\n');
    }

    [Fact]
    public void MoveNext_RadiusOne_ProducesCorrectNumberOfRows()
    {
        var topology = new HexagonalTopology(1);
        var layout = new HexLayout(topology);
        using var generation = new HexGeneration();

        var enumerator = new HexStaggeredTokenEnumerator(topology, layout, generation, ConsoleTheme.Default);
        int newlineCount = 0;

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.IsCharacter && enumerator.Current.Character == '\n')
            {
                newlineCount++;
            }
        }

        // Radius 1 = 3 rows (r = -1, 0, 1)
        newlineCount.ShouldBe(3);
    }

    [Fact]
    public void MoveNext_WithAliveCells_EmitsGreenColorSequences()
    {
        var topology = new HexagonalTopology(1);
        var layout = new HexLayout(topology);
        using var generation = new HexGeneration([default]);

        var enumerator = new HexStaggeredTokenEnumerator(topology, layout, generation, ConsoleTheme.Default);
        bool foundGreen = false;

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.IsSequence && enumerator.Current.Sequence == AnsiSequence.ForegroundGreen)
            {
                foundGreen = true;
                break;
            }
        }

        foundGreen.ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_WithDeadCells_EmitsDarkGrayColorSequences()
    {
        var topology = new HexagonalTopology(1);
        var layout = new HexLayout(topology);
        using var generation = new HexGeneration(); // All dead

        var enumerator = new HexStaggeredTokenEnumerator(topology, layout, generation, ConsoleTheme.Default);
        bool foundDarkGray = false;

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.IsSequence && enumerator.Current.Sequence == AnsiSequence.ForegroundDarkGray)
            {
                foundDarkGray = true;
                break;
            }
        }

        foundDarkGray.ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_UsesThemeCharacters()
    {
        var topology = new HexagonalTopology(0);
        var layout = new HexLayout(topology);
        using var generation = new HexGeneration([default]);
        ConsoleTheme theme = ConsoleTheme.Default;

        var enumerator = new HexStaggeredTokenEnumerator(topology, layout, generation, theme);
        bool foundAliveChar = false;

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.IsCharacter && enumerator.Current.Character == theme.AliveChar)
            {
                foundAliveChar = true;
                break;
            }
        }

        foundAliveChar.ShouldBeTrue();
    }

    [Fact]
    public void MoveNext_RadiusTwo_HasCorrectRowStructure()
    {
        var topology = new HexagonalTopology(2);
        var layout = new HexLayout(topology);
        using var generation = new HexGeneration();

        var enumerator = new HexStaggeredTokenEnumerator(topology, layout, generation, ConsoleTheme.Default);
        int newlineCount = 0;

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.IsCharacter && enumerator.Current.Character == '\n')
            {
                newlineCount++;
            }
        }

        // Radius 2 = 5 rows (r = -2, -1, 0, 1, 2)
        newlineCount.ShouldBe(5);
    }

    #endregion

    #region GetEnumerator Tests

    [Fact]
    public void GetEnumerator_CanEnumerateTokens()
    {
        var topology = new HexagonalTopology(1);
        var layout = new HexLayout(topology);
        using var generation = new HexGeneration();

        var enumerator = new HexStaggeredTokenEnumerator(topology, layout, generation, ConsoleTheme.Default);

        // Use foreach to verify GetEnumerator works (returns self for ref structs)
        int tokenCount = 0;
        foreach (Token token in enumerator)
        {
            _ = token; // Avoid unused variable warning
            tokenCount++;
            if (tokenCount > 100)
            {
                break; // Safety limit
            }
        }

        tokenCount.ShouldBeGreaterThan(0);
    }

    #endregion
}
