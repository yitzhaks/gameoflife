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
        var generation = new DictionaryGeneration<int, bool>(states, defaultState: false);

        Assert.True(generation[1]);
        Assert.False(generation[2]);
        Assert.True(generation[3]);
    }

    [Fact]
    public void Indexer_MissingNode_ReturnsDefaultState()
    {
        var states = new Dictionary<int, bool>
        {
            [1] = true
        };
        var generation = new DictionaryGeneration<int, bool>(states, defaultState: false);

        Assert.False(generation[99]); // Node not in dictionary
    }

    [Fact]
    public void Indexer_MissingNode_ReturnsTrueDefaultState()
    {
        var states = new Dictionary<int, bool>
        {
            [1] = false
        };
        var generation = new DictionaryGeneration<int, bool>(states, defaultState: true);

        Assert.True(generation[99]); // Missing node returns true default
    }

    [Fact]
    public void Indexer_EmptyDictionary_AlwaysReturnsDefaultState()
    {
        var states = new Dictionary<int, bool>();
        var generation = new DictionaryGeneration<int, bool>(states, defaultState: true);

        Assert.True(generation[0]);
        Assert.True(generation[1]);
        Assert.True(generation[-1]);
    }

    #endregion

    #region Indexer Tests with Point2D TIdentity

    [Fact]
    public void Indexer_Point2DStoredNode_ReturnsStoredState()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(0, 0)] = true,
            [new Point2D(1, 1)] = false,
            [new Point2D(2, 3)] = true
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        Assert.True(generation[new Point2D(0, 0)]);
        Assert.False(generation[new Point2D(1, 1)]);
        Assert.True(generation[new Point2D(2, 3)]);
    }

    [Fact]
    public void Indexer_Point2DMissingNode_ReturnsDefaultState()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(0, 0)] = true
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        Assert.False(generation[new Point2D(99, 99)]); // Node not in dictionary
    }

    [Fact]
    public void Indexer_Point2DNegativeCoordinates_WorksCorrectly()
    {
        var states = new Dictionary<Point2D, bool>
        {
            [new Point2D(-1, -2)] = true
        };
        var generation = new DictionaryGeneration<Point2D, bool>(states, defaultState: false);

        Assert.True(generation[new Point2D(-1, -2)]);
        Assert.False(generation[new Point2D(-1, -3)]); // Different coordinate, should return default
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
        var generation = new DictionaryGeneration<string, int>(states, defaultState: -1);

        Assert.Equal(1, generation["alive"]);
        Assert.Equal(0, generation["dead"]);
        Assert.Equal(-1, generation["unknown"]);
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
        var generation = new DictionaryGeneration<int, int>(states, defaultState: 0);

        Assert.Equal(100, generation[1]);
        Assert.Equal(200, generation[2]);
        Assert.Equal(0, generation[3]); // Missing
    }

    [Fact]
    public void Indexer_StringState_WorksCorrectly()
    {
        var states = new Dictionary<int, string>
        {
            [1] = "alive",
            [2] = "dead"
        };
        var generation = new DictionaryGeneration<int, string>(states, defaultState: "unknown");

        Assert.Equal("alive", generation[1]);
        Assert.Equal("dead", generation[2]);
        Assert.Equal("unknown", generation[3]); // Missing
    }

    [Fact]
    public void Indexer_NullableReferenceTypeState_WorksWithNullDefault()
    {
        var states = new Dictionary<int, string?>
        {
            [1] = "value"
        };
        var generation = new DictionaryGeneration<int, string?>(states, defaultState: null);

        Assert.Equal("value", generation[1]);
        Assert.Null(generation[2]); // Missing, returns null default
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullDictionary_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
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
        var generation = new DictionaryGeneration<int, bool>(states, defaultState: false);

        // Modify the original dictionary after construction
        states[1] = false;
        states[2] = true;

        // Generation should still have the original values
        Assert.True(generation[1]); // Should still be true, not false
        Assert.False(generation[2]); // Should be default (false), not true
    }

    #endregion

    #region Interface Implementation

    [Fact]
    public void ImplementsIGeneration()
    {
        var states = new Dictionary<int, bool>();
        var generation = new DictionaryGeneration<int, bool>(states, defaultState: false);

        Assert.IsAssignableFrom<IGeneration<int, bool>>(generation);
    }

    #endregion
}
