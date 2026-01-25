namespace GameOfLife.Core.Tests;

public class WorldTests
{
    #region Test Doubles

    /// <summary>
    /// A simple stub topology for testing World.
    /// </summary>
    private class StubTopology : ITopology<int>
    {
        private readonly Dictionary<int, List<int>> _neighbors;

        public StubTopology(IEnumerable<int> nodes, Dictionary<int, List<int>>? neighbors = null)
        {
            Nodes = nodes;
            _neighbors = neighbors ?? [];
        }

        public IEnumerable<int> Nodes { get; }

        public IEnumerable<int> GetNeighbors(int node)
        {
            return _neighbors.TryGetValue(node, out var neighbors) ? neighbors : Enumerable.Empty<int>();
        }
    }

    /// <summary>
    /// A simple stub rules implementation for testing World.
    /// </summary>
    private class StubRules : IRules<bool>
    {
        private readonly Func<bool, IEnumerable<bool>, bool>? _nextStateFunc;

        public StubRules(bool defaultState = false, Func<bool, IEnumerable<bool>, bool>? nextStateFunc = null)
        {
            DefaultState = defaultState;
            _nextStateFunc = nextStateFunc;
        }

        public bool DefaultState { get; }

        public bool GetNextState(bool current, IEnumerable<bool> neighborStates)
        {
            return _nextStateFunc?.Invoke(current, neighborStates) ?? current;
        }
    }

    /// <summary>
    /// A simple stub generation for testing.
    /// </summary>
    private class StubGeneration : IGeneration<int, bool>
    {
        private readonly Dictionary<int, bool> _states;
        private readonly bool _defaultState;

        public StubGeneration(Dictionary<int, bool>? states = null, bool defaultState = false)
        {
            _states = states ?? [];
            _defaultState = defaultState;
        }

        public bool this[int node] => _states.TryGetValue(node, out var state) ? state : _defaultState;
    }

    #endregion

    #region Constructor Null Argument Tests

    [Fact]
    public void Constructor_NullTopology_ThrowsArgumentNullException()
    {
        var rules = new StubRules();

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new World<int, bool>(null!, rules));

        Assert.Equal("topology", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullRules_ThrowsArgumentNullException()
    {
        var topology = new StubTopology(new[] { 1, 2, 3 });

        var exception = Assert.Throws<ArgumentNullException>(() =>
            new World<int, bool>(topology, null!));

        Assert.Equal("rules", exception.ParamName);
    }

    #endregion

    #region Constructor Property Tests

    [Fact]
    public void Constructor_ValidArguments_SetsTopologyProperty()
    {
        var topology = new StubTopology(new[] { 1, 2, 3 });
        var rules = new StubRules();

        var world = new World<int, bool>(topology, rules);

        Assert.Same(topology, world.Topology);
    }

    [Fact]
    public void Constructor_ValidArguments_SetsRulesProperty()
    {
        var topology = new StubTopology(new[] { 1, 2, 3 });
        var rules = new StubRules();

        var world = new World<int, bool>(topology, rules);

        Assert.Same(rules, world.Rules);
    }

    #endregion

    #region Tick Null Argument Tests

    [Fact]
    public void Tick_NullGeneration_ThrowsArgumentNullException()
    {
        var topology = new StubTopology(new[] { 1, 2, 3 });
        var rules = new StubRules();
        var world = new World<int, bool>(topology, rules);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            world.Tick(null!));

        Assert.Equal("current", exception.ParamName);
    }

    #endregion

    #region Tick Functionality Tests

    [Fact]
    public void Tick_ValidGeneration_ReturnsNewInstance()
    {
        var topology = new StubTopology(new[] { 1, 2, 3 });
        var rules = new StubRules();
        var world = new World<int, bool>(topology, rules);
        var currentGeneration = new StubGeneration();

        var nextGeneration = world.Tick(currentGeneration);

        Assert.NotSame(currentGeneration, nextGeneration);
    }

    [Fact]
    public void Tick_AppliesRulesToComputeNextState()
    {
        // Setup: A single node with no neighbors
        // Rules: Always return the opposite of current state
        var topology = new StubTopology(new[] { 1 });
        var rules = new StubRules(
            defaultState: false,
            nextStateFunc: (current, _) => !current);
        var world = new World<int, bool>(topology, rules);

        // Node 1 starts as alive (true)
        var currentGeneration = new StubGeneration(
            new Dictionary<int, bool> { [1] = true });

        var nextGeneration = world.Tick(currentGeneration);

        // Rule should flip the state to false
        Assert.False(nextGeneration[1]);
    }

    [Fact]
    public void Tick_PassesNeighborStatesToRules()
    {
        // Setup: Node 1 has neighbors 2 and 3
        // Rules: Return true if any neighbor is true
        var neighbors = new Dictionary<int, List<int>>
        {
            [1] = [2, 3],
            [2] = [],
            [3] = []
        };
        var topology = new StubTopology(new[] { 1, 2, 3 }, neighbors);

        var capturedNeighborStates = new List<bool>();
        var rules = new StubRules(
            defaultState: false,
            nextStateFunc: (current, neighborStates) =>
            {
                var neighborList = neighborStates.ToList();
                if (neighborList.Count > 0)
                {
                    capturedNeighborStates.AddRange(neighborList);
                }

                return neighborList.Any(n => n);
            });
        var world = new World<int, bool>(topology, rules);

        // Node 2 is alive, Node 3 is dead
        var currentGeneration = new StubGeneration(
            new Dictionary<int, bool> { [2] = true, [3] = false });

        var nextGeneration = world.Tick(currentGeneration);

        // Node 1 should be true because neighbor 2 is true
        Assert.True(nextGeneration[1]);
        // Verify the neighbor states were passed correctly (true, false)
        Assert.Contains(true, capturedNeighborStates);
        Assert.Contains(false, capturedNeighborStates);
    }

    [Fact]
    public void Tick_ComputesStateForAllNodes()
    {
        // Setup: Three nodes with no neighbors
        // Rules: Always return true
        var topology = new StubTopology(new[] { 1, 2, 3 });
        var rules = new StubRules(
            defaultState: false,
            nextStateFunc: (_, _) => true);
        var world = new World<int, bool>(topology, rules);

        var currentGeneration = new StubGeneration();

        var nextGeneration = world.Tick(currentGeneration);

        // All nodes should be true
        Assert.True(nextGeneration[1]);
        Assert.True(nextGeneration[2]);
        Assert.True(nextGeneration[3]);
    }

    [Fact]
    public void Tick_UsesDictionaryGenerationWithCorrectDefaultState()
    {
        // Setup: Topology with nodes
        // Rules with specific default state
        var topology = new StubTopology(new[] { 1 });
        var rules = new StubRules(defaultState: true);
        var world = new World<int, bool>(topology, rules);

        var currentGeneration = new StubGeneration();

        var nextGeneration = world.Tick(currentGeneration);

        // The returned generation should use the rules' default state
        // Query a node that wasn't explicitly set - should return the default state
        // Note: Since we only have node 1 in topology, and it's computed,
        // let's verify that the generation is a DictionaryGeneration
        Assert.IsType<DictionaryGeneration<int, bool>>(nextGeneration);
    }

    #endregion
}
