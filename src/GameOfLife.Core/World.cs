namespace GameOfLife.Core;

/// <summary>
/// The "engine" that combines topology with rules to compute ticks.
/// Stateless - computes next generations from input without holding state.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TState">The type representing cell state.</typeparam>
public class World<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    /// <summary>
    /// Gets the topology defining the structure of the world.
    /// </summary>
    public ITopology<TIdentity> Topology { get; }

    /// <summary>
    /// Gets the rules defining state transitions.
    /// </summary>
    public IRules<TState> Rules { get; }

    /// <summary>
    /// Creates a new world with the specified topology and rules.
    /// </summary>
    /// <param name="topology">The topology defining structure.</param>
    /// <param name="rules">The rules defining state transitions.</param>
    public World(ITopology<TIdentity> topology, IRules<TState> rules)
    {
        Topology = topology ?? throw new ArgumentNullException(nameof(topology));
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    /// <summary>
    /// Computes the next generation by applying rules to each node.
    /// </summary>
    /// <param name="current">The current generation state.</param>
    /// <returns>A new generation representing the next state.</returns>
    public IGeneration<TIdentity, TState> Tick(IGeneration<TIdentity, TState> current)
    {
        ArgumentNullException.ThrowIfNull(current);

        var nextStates = new Dictionary<TIdentity, TState>();

        foreach (var node in Topology.Nodes)
        {
            var currentState = current[node];
            var neighborStates = Topology.GetNeighbors(node).Select(neighbor => current[neighbor]);
            var nextState = Rules.GetNextState(currentState, neighborStates);
            nextStates[node] = nextState;
        }

        return new DictionaryGeneration<TIdentity, TState>(nextStates, Rules.DefaultState);
    }
}
