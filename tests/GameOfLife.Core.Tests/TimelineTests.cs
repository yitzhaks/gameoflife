using AutoFixture.Xunit2;

using NSubstitute;

using Shouldly;

namespace GameOfLife.Core.Tests;

public class TimelineTests
{
    #region Test Helpers

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

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_InitialState_IsAccessibleViaCurrent(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns(new[] { 0, 1, 2 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 10, [1] = 20, [2] = 30 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);

        // Act
        using var timeline = Timeline.Create(world, initial);

        // Assert
        timeline.Current.ShouldBeSameAs(initial);
        timeline.Current[0].ShouldBe(10);
        timeline.Current[1].ShouldBe(20);
        timeline.Current[2].ShouldBe(30);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_World_IsAccessible(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        using var initial = new DictionaryGeneration<int, int>(new Dictionary<int, int>(), 0);

        // Act
        using var timeline = Timeline.Create(world, initial);

        // Assert
        timeline.World.ShouldBeSameAs(world);
    }

    [Fact]
    public void Constructor_NullWorld_ThrowsArgumentNullException()
    {
        // Arrange
        using var initial = new DictionaryGeneration<int, int>(new Dictionary<int, int>(), 0);

        // Act & Assert
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => new Timeline<int, int, IGeneration<int, int>>(null!, initial));
        ex.ParamName.ShouldBe("world");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_NullInitial_ThrowsArgumentNullException(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);

        // Act & Assert
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => new Timeline<int, int, IGeneration<int, int>>(world, null!));
        ex.ParamName.ShouldBe("value");
    }

    [Theory]
    [AutoNSubstituteData]
    public void Step_AdvancesOneGeneration(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns(new[] { 0, 1 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0, [1] = 5 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step();

        // Assert
        timeline.Current.ShouldNotBeSameAs(initial);
        timeline.Current[0].ShouldBe(1); // 0 + 1
        timeline.Current[1].ShouldBe(6); // 5 + 1
    }

    [Theory]
    [AutoNSubstituteData]
    public void Step_CalledTwice_AdvancesTwoGenerations(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns(new[] { 0 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step();
        timeline.Step();

        // Assert
        timeline.Current[0].ShouldBe(2);
    }

    [Theory]
    [AutoNSubstituteData]
    public void StepWithCount_AdvancesNGenerations(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns(new[] { 0 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step(5);

        // Assert
        timeline.Current[0].ShouldBe(5);
    }

    [Theory]
    [AutoNSubstituteData]
    public void StepWithCount_ZeroCount_IsNoOp(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns(new[] { 0 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 42 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step(0);

        // Assert
        timeline.Current.ShouldBeSameAs(initial);
        timeline.Current[0].ShouldBe(42);
    }

    [Theory]
    [AutoNSubstituteData]
    public void StepWithCount_NegativeCount_IsNoOp(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns(new[] { 0 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 42 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step(-5);

        // Assert
        timeline.Current.ShouldBeSameAs(initial);
        timeline.Current[0].ShouldBe(42);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Step_WithNeighborInteraction_AppliesRulesCorrectly(ITopology<int> topology)
    {
        // Arrange: Node 0 has neighbors 1 and 2, so after tick its state should be 1 + 2 = 3
        _ = topology.Nodes.Returns(new[] { 0, 1, 2 });
        _ = topology.GetNeighbors(0).Returns(new[] { 1, 2 });
        _ = topology.GetNeighbors(1).Returns([]);
        _ = topology.GetNeighbors(2).Returns([]);
        var rules = new SumNeighborsRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0, [1] = 1, [2] = 2 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step();

        // Assert
        timeline.Current[0].ShouldBe(3); // sum of neighbors: 1 + 2 = 3
        timeline.Current[1].ShouldBe(0); // no neighbors
        timeline.Current[2].ShouldBe(0); // no neighbors
    }

    [Theory]
    [AutoNSubstituteData]
    public void StepWithCount_LargeCount_WorksCorrectly(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns(new[] { 0 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);
        var rules = new CountingRules();
        var world = new World<int, int>(topology, rules);
        var initialStates = new Dictionary<int, int> { [0] = 0 };
        using var initial = new DictionaryGeneration<int, int>(initialStates, 0);
        using var timeline = Timeline.Create(world, initial);

        // Act
        timeline.Step(100);

        // Assert
        timeline.Current[0].ShouldBe(100);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Step_CurrentPropertyUpdates_AfterEachStep(ITopology<int> topology)
    {
        // Arrange
        _ = topology.Nodes.Returns(new[] { 0 });
        _ = topology.GetNeighbors(Arg.Any<int>()).Returns([]);
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
        generations.Count.ShouldBe(4);
        generations[0][0].ShouldBe(0);
        generations[1][0].ShouldBe(1);
        generations[2][0].ShouldBe(2);
        generations[3][0].ShouldBe(3);
    }
}
