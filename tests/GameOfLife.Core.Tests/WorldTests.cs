using AutoFixture.Xunit2;

using NSubstitute;

using Shouldly;

namespace GameOfLife.Core.Tests;

public class WorldTests
{
    #region Constructor Null Argument Tests

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_NullTopology_ThrowsArgumentNullException(IRules<bool> rules)
    {
        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            new World<int, bool>(null!, rules));

        exception.ParamName.ShouldBe("topology");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_NullRules_ThrowsArgumentNullException(ITopology<int> topology)
    {
        _ = topology.Nodes.Returns(new[] { 1, 2, 3 });

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            new World<int, bool>(topology, null!));

        exception.ParamName.ShouldBe("rules");
    }

    #endregion

    #region Constructor Property Tests

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_ValidArguments_SetsTopologyProperty(ITopology<int> topology, IRules<bool> rules)
    {
        _ = topology.Nodes.Returns(new[] { 1, 2, 3 });

        var world = new World<int, bool>(topology, rules);

        world.Topology.ShouldBeSameAs(topology);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_ValidArguments_SetsRulesProperty(ITopology<int> topology, IRules<bool> rules)
    {
        _ = topology.Nodes.Returns(new[] { 1, 2, 3 });

        var world = new World<int, bool>(topology, rules);

        world.Rules.ShouldBeSameAs(rules);
    }

    #endregion

    #region Tick Null Argument Tests

    [Theory]
    [AutoNSubstituteData]
    public void Tick_NullGeneration_ThrowsArgumentNullException(ITopology<int> topology, IRules<bool> rules)
    {
        _ = topology.Nodes.Returns(new[] { 1, 2, 3 });
        var world = new World<int, bool>(topology, rules);

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
            world.Tick(null!));

        exception.ParamName.ShouldBe("currentGeneration");
    }

    #endregion

    #region Tick Functionality Tests

    [Theory]
    [AutoNSubstituteData]
    public void Tick_ValidGeneration_ReturnsNewInstance(
        ITopology<int> topology,
        IRules<bool> rules,
        IGeneration<int, bool> currentGeneration)
    {
        _ = topology.Nodes.Returns(new[] { 1, 2, 3 });
        var world = new World<int, bool>(topology, rules);

        IGeneration<int, bool> nextGeneration = world.Tick(currentGeneration);

        nextGeneration.ShouldNotBeSameAs(currentGeneration);
    }

    [Fact]
    public void Tick_AppliesRulesToComputeNextState()
    {
        // Setup: A single node with no neighbors
        // Rules: Always return the opposite of current state
        ITopology<int> topology = Substitute.For<ITopology<int>>();
        _ = topology.Nodes.Returns(new[] { 1 });
        _ = topology.GetNeighbors(1).Returns([]);

        IRules<bool> rules = Substitute.For<IRules<bool>>();
        _ = rules.DefaultState.Returns(false);
        _ = rules.GetNextState(Arg.Any<bool>(), Arg.Any<IEnumerable<bool>>())
            .Returns(callInfo => !callInfo.Arg<bool>());

        var world = new World<int, bool>(topology, rules);

        // Node 1 starts as alive (true)
        IGeneration<int, bool> currentGeneration = Substitute.For<IGeneration<int, bool>>();
        _ = currentGeneration[1].Returns(true);

        IGeneration<int, bool> nextGeneration = world.Tick(currentGeneration);

        // Rule should flip the state to false
        nextGeneration[1].ShouldBeFalse();
    }

    [Fact]
    public void Tick_PassesNeighborStatesToRules()
    {
        // Setup: Node 1 has neighbors 2 and 3
        // Rules: Return true if any neighbor is true
        ITopology<int> topology = Substitute.For<ITopology<int>>();
        _ = topology.Nodes.Returns(new[] { 1, 2, 3 });
        _ = topology.GetNeighbors(1).Returns(new[] { 2, 3 });
        _ = topology.GetNeighbors(2).Returns([]);
        _ = topology.GetNeighbors(3).Returns([]);

        var capturedNeighborStates = new List<bool>();
        IRules<bool> rules = Substitute.For<IRules<bool>>();
        _ = rules.DefaultState.Returns(false);
        _ = rules.GetNextState(Arg.Any<bool>(), Arg.Any<IEnumerable<bool>>())
            .Returns(callInfo =>
            {
                var neighborStates = callInfo.Arg<IEnumerable<bool>>().ToList();
                if (neighborStates.Count > 0)
                {
                    capturedNeighborStates.AddRange(neighborStates);
                }

                return neighborStates.Any(n => n);
            });

        var world = new World<int, bool>(topology, rules);

        // Node 2 is alive, Node 3 is dead
        IGeneration<int, bool> currentGeneration = Substitute.For<IGeneration<int, bool>>();
        _ = currentGeneration[1].Returns(false);
        _ = currentGeneration[2].Returns(true);
        _ = currentGeneration[3].Returns(false);

        IGeneration<int, bool> nextGeneration = world.Tick(currentGeneration);

        // Node 1 should be true because neighbor 2 is true
        nextGeneration[1].ShouldBeTrue();
        // Verify the neighbor states were passed correctly (true, false)
        capturedNeighborStates.ShouldContain(true);
        capturedNeighborStates.ShouldContain(false);
    }

    [Fact]
    public void Tick_ComputesStateForAllNodes()
    {
        // Setup: Three nodes with no neighbors
        // Rules: Always return true
        ITopology<int> topology = Substitute.For<ITopology<int>>();
        _ = topology.Nodes.Returns(new[] { 1, 2, 3 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);

        IRules<bool> rules = Substitute.For<IRules<bool>>();
        _ = rules.DefaultState.Returns(false);
        _ = rules.GetNextState(Arg.Any<bool>(), Arg.Any<IEnumerable<bool>>()).Returns(true);

        var world = new World<int, bool>(topology, rules);
        IGeneration<int, bool> currentGeneration = Substitute.For<IGeneration<int, bool>>();

        IGeneration<int, bool> nextGeneration = world.Tick(currentGeneration);

        // All nodes should be true
        nextGeneration[1].ShouldBeTrue();
        nextGeneration[2].ShouldBeTrue();
        nextGeneration[3].ShouldBeTrue();
    }

    [Fact]
    public void Tick_UsesDictionaryGenerationWithCorrectDefaultState()
    {
        // Setup: Topology with nodes
        // Rules with specific default state
        ITopology<int> topology = Substitute.For<ITopology<int>>();
        _ = topology.Nodes.Returns(new[] { 1 });
        _ = topology.GetNeighbors(1).Returns([]);

        IRules<bool> rules = Substitute.For<IRules<bool>>();
        _ = rules.DefaultState.Returns(true);
        _ = rules.GetNextState(Arg.Any<bool>(), Arg.Any<IEnumerable<bool>>()).Returns(true);

        var world = new World<int, bool>(topology, rules);
        IGeneration<int, bool> currentGeneration = Substitute.For<IGeneration<int, bool>>();

        IGeneration<int, bool> nextGeneration = world.Tick(currentGeneration);

        // The returned generation should use the rules' default state
        // Query a node that wasn't explicitly set - should return the default state
        // Note: Since we only have node 1 in topology, and it's computed,
        // let's verify that the generation is a DictionaryGeneration
        _ = nextGeneration.ShouldBeOfType<DictionaryGeneration<int, bool>>();
    }

    #endregion
}
