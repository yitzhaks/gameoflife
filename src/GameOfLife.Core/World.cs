namespace GameOfLife.Core;

public sealed class World<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    public World(ITopology<TIdentity> topology, IRules<TState> rules)
    {
        Topology = topology ?? throw new ArgumentNullException(nameof(topology));
        Rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    public ITopology<TIdentity> Topology { get; }
    public IRules<TState> Rules { get; }

    public IGeneration<TIdentity, TState> Tick(IGeneration<TIdentity, TState> current)
    {
        ArgumentNullException.ThrowIfNull(current);

        if (Topology is GridTopology gridTopology && typeof(TIdentity) == typeof(Point2D) && typeof(TState) == typeof(bool))
        {
            var aliveCells = new List<Point2D>();
            foreach (var node in Topology.Nodes)
            {
                var neighborStates = Topology.GetNeighbors(node).Select(neighbor => current[neighbor]);
                var next = Rules.GetNextState(current[node], neighborStates);
                if (next is bool isAlive && isAlive)
                {
                    aliveCells.Add((Point2D)(object)node);
                }
            }

            return (IGeneration<TIdentity, TState>)(object)new DenseGeneration(gridTopology.Width, gridTopology.Height, aliveCells);
        }

        var states = new Dictionary<TIdentity, TState>();
        foreach (var node in Topology.Nodes)
        {
            var neighborStates = Topology.GetNeighbors(node).Select(neighbor => current[neighbor]);
            var next = Rules.GetNextState(current[node], neighborStates);
            states[node] = next;
        }

        return new DictionaryGeneration<TIdentity, TState>(states);
    }

    private sealed class DictionaryGeneration<TNode, TValue> : IGeneration<TNode, TValue>
        where TNode : notnull, IEquatable<TNode>
    {
        private readonly IReadOnlyDictionary<TNode, TValue> _states;

        public DictionaryGeneration(IReadOnlyDictionary<TNode, TValue> states)
        {
            _states = states ?? throw new ArgumentNullException(nameof(states));
        }

        public TValue this[TNode node]
        {
            get
            {
                if (!_states.TryGetValue(node, out var value))
                {
                    throw new ArgumentOutOfRangeException(nameof(node), "Node is not part of this generation.");
                }

                return value;
            }
        }
    }
}
