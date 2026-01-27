namespace GameOfLife.Core.Tests;

public class TimelineTests
{
    #region Test Helpers

    /// <summary>
    /// A simple topology for testing that contains a fixed set of nodes with configurable neighbors.
    /// </summary>
    private class TestTopology : ITopology<int>
    {
        private readonly Dictionary<int, List<int>> _neighbors;

        public TestTopology(int nodeCount)
        {
            Nodes = Enumerable.Range(0, nodeCount).ToList();
            _neighbors = Nodes.ToDictionary(n => n, _ => new List<int>());
        }

        public IEnumerable<int> Nodes { get; }

        public void AddNeighbor(int from, int to) => _neighbors[from].Add(to);

        public IEnumerable<int> GetNeighbors(int node) => _neighbors[node];
    }

    /// <summary>
    /// Simple rules for testing: increments the count each tick.
    /// </summary>
    private class CountingRules : IRules<int>
    {
        public int DefaultState => 0;

        public int GetNextState(int current, IEnumerable<int> neighborStates) => current + 1;
    }

    /// <summary>
    /// Rules that sum neighbor states into the new state.
    /// </summary>
    private class SumNeighborsRules : IRules<int>
    {
        public int DefaultState => 0;

        public int GetNextState(int current, IEnumerable<int> neighborStates) => neighborStates.Sum();
    }

    #endregion

    [Fact]
    public void Constructor_InitialState_IsAccessibleViaCurrent()
    {
        // Arrange
        var topology = new TestTopology(3);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 10, [1] = 20, [2] = 30 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);

        // Act
        using var timeline = Timeline.Create(world, initial);

        // Assert
        Assert.Same(initial, timeline.Current);
        Assert.Equal(10, timeline.Current[0]);
        Assert.Equal(20, timeline.Current[1]);
        Assert.Equal(30, timeline.Current[2]);
    }

    [Fact]
    public void Constructor_World_IsAccessible()
    {
        // Arrange
        var topology = new TestTopology(1);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        using var initial = new DictionaryGeneration<int, int>(new Dictionary<int, int>(), 0);

        // Act
        using var timeline = Timeline.Create(world, initial);

        // Assert
        Assert.Same(world, timeline.World);
    }

    [Fact]
    public void Constructor_NullWorld_ThrowsArgumentNullException()
    {
        // Arrange
        using var initial = new DictionaryGeneration<int, int>(new Dictionary<int, int>(), 0);

        // Act & Assert
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new Timeline<int, int, IGeneration<int, int>>(null!, initial));
        Assert.Equal("world", ex.ParamName);
    }

    [Fact]
    public void Constructor_NullInitial_ThrowsArgumentNullException()
    {
        // Arrange
        var topology = new TestTopology(1);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);

        // Act & Assert
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => new Timeline<int, int, IGeneration<int, int>>(world, null!));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void Step_AdvancesOneGeneration()
    {
        // Arrange
        var topology = new TestTopology(2);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0, [1] = 5 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step();

        // Assert
        Assert.NotSame(initial, timeline.Current);
        Assert.Equal(1, timeline.Current[0]); // 0 + 1
        Assert.Equal(6, timeline.Current[1]); // 5 + 1
    }

    [Fact]
    public void Step_CalledTwice_AdvancesTwoGenerations()
    {
        // Arrange
        var topology = new TestTopology(1);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step();
        timeline.Step();

        // Assert
        Assert.Equal(2, timeline.Current[0]);
    }

    [Fact]
    public void StepWithCount_AdvancesNGenerations()
    {
        // Arrange
        var topology = new TestTopology(1);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step(5);

        // Assert
        Assert.Equal(5, timeline.Current[0]);
    }

    [Fact]
    public void StepWithCount_ZeroCount_IsNoOp()
    {
        // Arrange
        var topology = new TestTopology(1);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 42 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step(0);

        // Assert
        Assert.Same(initial, timeline.Current);
        Assert.Equal(42, timeline.Current[0]);
    }

    [Fact]
    public void StepWithCount_NegativeCount_IsNoOp()
    {
        // Arrange
        var topology = new TestTopology(1);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 42 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step(-5);

        // Assert
        Assert.Same(initial, timeline.Current);
        Assert.Equal(42, timeline.Current[0]);
    }

    [Fact]
    public void Step_WithNeighborInteraction_AppliesRulesCorrectly()
    {
        // Arrange: Node 0 has neighbors 1 and 2, so after tick its state should be 1 + 2 = 3
        var topology = new TestTopology(3);
        topology.AddNeighbor(0, 1);
        topology.AddNeighbor(0, 2);
        var rules = new SumNeighborsRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0, [1] = 1, [2] = 2 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step();

        // Assert
        Assert.Equal(3, timeline.Current[0]); // sum of neighbors: 1 + 2 = 3
        Assert.Equal(0, timeline.Current[1]); // no neighbors
        Assert.Equal(0, timeline.Current[2]); // no neighbors
    }

    [Fact]
    public void StepWithCount_LargeCount_WorksCorrectly()
    {
        // Arrange
        var topology = new TestTopology(1);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step(100);

        // Assert
        Assert.Equal(100, timeline.Current[0]);
    }

    [Fact]
    public void Step_CurrentPropertyUpdates_AfterEachStep()
    {
        // Arrange
        var topology = new TestTopology(1);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        var generations = new List<IGeneration<int, int>> { timeline.Current };

        // Act
        for (int i = 0; i < 3; i++)
        {
            timeline.Step();
            generations.Add(timeline.Current);
        }

        // Assert
        Assert.Equal(4, generations.Count);
        Assert.Equal(0, generations[0][0]);
        Assert.Equal(1, generations[1][0]);
        Assert.Equal(2, generations[2][0]);
        Assert.Equal(3, generations[3][0]);
    }
}
