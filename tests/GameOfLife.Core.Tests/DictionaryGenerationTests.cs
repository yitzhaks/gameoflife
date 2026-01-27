using Shouldly;

namespace GameOfLife.Core.Tests;

public class DictionaryGenerationTests
{
    #region Indexer Tests with int TIdentity

    [Fact]
    public void Indexer_StoredNode_ReturnsStoredState()
    {
        var states = new Dictionary<int, bool>
        {
            [1] = true,
            [2] = false,
            [3] = true
        };
        using var generation = new DictionaryGeneration<int, bool>(states, defaultState: false);

        generation[1].ShouldBeTrue();
        generation[2].ShouldBeFalse();
        generation[3].ShouldBeTrue();
    }

    [Fact]
    public void Indexer_MissingNode_ReturnsDefaultState()
    {
        var states = new Dictionary<int, bool>
        {
            [1] = true
        };
        using var generation = new DictionaryGeneration<int, bool>(states, defaultState: false);

        generation[99].ShouldBeFalse(); // Node not in dictionary
    }

    [Fact]
    public void Indexer_MissingNode_ReturnsTrueDefaultState()
    {
        var states = new Dictionary<int, bool>
        {
            [1] = false
        };
        using var generation = new DictionaryGeneration<int, bool>(states, defaultState: true);

        generation[99].ShouldBeTrue(); // Missing node returns true default
    }

    [Fact]
    public void Indexer_EmptyDictionary_AlwaysReturnsDefaultState()
    {
        var states = new Dictionary<int, bool>();
        using var generation = new DictionaryGeneration<int, bool>(states, defaultState: true);

        generation[0].ShouldBeTrue();
        generation[1].ShouldBeTrue();
        generation[-1].ShouldBeTrue();
    }

    #endregion

    #region Indexer Tests with Point2D TIdentity

    [Fact]
    public void Indexer_Point2DStoredNode_ReturnsStoredState()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true,
            [(1, 1)] = false,
            [(2, 3)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        generation[default].ShouldBeTrue();
        generation[(1, 1)].ShouldBeFalse();
        generation[(2, 3)].ShouldBeTrue();
    }

    [Fact]
    public void Indexer_Point2DMissingNode_ReturnsDefaultState()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [default] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        generation[(99, 99)].ShouldBeFalse(); // Node not in dictionary
    }

    [Fact]
    public void Indexer_Point2DNegativeCoordinates_WorksCorrectly()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [(-1, -2)] = true
        };
        using var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        generation[(-1, -2)].ShouldBeTrue();
        generation[(-1, -3)].ShouldBeFalse(); // Different coordinate, should return default
    }

    #endregion

    #region Indexer Tests with string TIdentity

    [Fact]
    public void Indexer_StringIdentity_WorksCorrectly()
    {
        var states = new Dictionary<string, int>
        {
            ["alive"] = 1,
            ["dead"] = 0
        };
        using var generation = new DictionaryGeneration<string, int>(states, defaultState: -1);

        generation["alive"].ShouldBe(1);
        generation["dead"].ShouldBe(0);
        generation["unknown"].ShouldBe(-1);
    }

    #endregion

    #region Different TState Types

    [Fact]
    public void Indexer_IntState_WorksCorrectly()
    {
        var states = new Dictionary<int, int>
        {
            [1] = 100,
            [2] = 200
        };
        using var generation = new DictionaryGeneration<int, int>(states, defaultState: 0);

        generation[1].ShouldBe(100);
        generation[2].ShouldBe(200);
        generation[3].ShouldBe(0); // Missing
    }

    [Fact]
    public void Indexer_StringState_WorksCorrectly()
    {
        var states = new Dictionary<int, string>
        {
            [1] = "alive",
            [2] = "dead"
        };
        using var generation = new DictionaryGeneration<int, string>(states, defaultState: "unknown");

        generation[1].ShouldBe("alive");
        generation[2].ShouldBe("dead");
        generation[3].ShouldBe("unknown"); // Missing
    }

    [Fact]
    public void Indexer_NullableReferenceTypeState_WorksWithNullDefault()
    {
        var states = new Dictionary<int, string?>
        {
            [1] = "value"
        };
        using var generation = new DictionaryGeneration<int, string?>(states, defaultState: null);

        generation[1].ShouldBe("value");
        generation[2].ShouldBeNull(); // Missing, returns null default
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullDictionary_ThrowsArgumentNullException()
    {
        _ = Should.Throw<ArgumentNullException>(() =>
            new DictionaryGeneration<int, bool>(null!, defaultState: false));
    }

    #endregion

    #region Defensive Copy Tests

    [Fact]
    public void Constructor_DefensivelyCopiesDictionary_ModifyingOriginalDoesNotAffectGeneration()
    {
        var states = new Dictionary<int, bool>
        {
            [1] = true
        };
        using var generation = new DictionaryGeneration<int, bool>(states, defaultState: false);

        // Modify the original dictionary after construction
        states[1] = false;
        states[2] = true;

        // Generation should still have the original values
        generation[1].ShouldBeTrue(); // Should still be true, not false
        generation[2].ShouldBeFalse(); // Should be default (false), not true
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ImplementsIGeneration()
    {
        var states = new Dictionary<int, bool>();
        using var generation = new DictionaryGeneration<int, bool>(states, defaultState: false);

        _ = generation.ShouldBeAssignableTo<IGeneration<int, bool>>();
    }

    #endregion
}
