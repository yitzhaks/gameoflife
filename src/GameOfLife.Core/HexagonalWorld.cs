namespace GameOfLife.Core;

/// <summary>
/// World implementation for hexagonal grids with 6-neighbor connectivity.
/// Uses sparse HashSet-based state storage for efficient operation on hex boards.
/// </summary>
public class HexagonalWorld : IWorld<HexPoint, bool, IGeneration<HexPoint, bool>>
{
    private readonly ICellularAutomatonRules _rules;

    /// <summary>
    /// Creates a new hexagonal world with the specified radius and classic rules.
    /// </summary>
    /// <param name="radius">The radius of the hexagonal topology.</param>
    public HexagonalWorld(int radius) : this(radius, new ClassicRules())
    {
    }

    /// <summary>
    /// Creates a new hexagonal world with the specified radius and rules.
    /// </summary>
    /// <param name="radius">The radius of the hexagonal topology.</param>
    /// <param name="rules">The cellular automata rules to apply.</param>
    public HexagonalWorld(int radius, ICellularAutomatonRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        Radius = radius;
        Topology = new HexagonalTopology(radius);
        _rules = rules;
    }

    /// <summary>
    /// Gets the radius of the hexagonal topology.
    /// </summary>
    public int Radius { get; }

    /// <summary>
    /// Gets the topology.
    /// </summary>
    public HexagonalTopology Topology { get; }

    /// <inheritdoc />
    ITopology<HexPoint> IWorld<HexPoint, bool, IGeneration<HexPoint, bool>>.Topology => Topology;

    /// <inheritdoc />
    public IGeneration<HexPoint, bool> Tick(IGeneration<HexPoint, bool> currentGeneration)
    {
        ArgumentNullException.ThrowIfNull(currentGeneration);

        var nextAlive = new List<HexPoint>();

        // Compute next state for each cell
        foreach (HexPoint cell in Topology.Nodes)
        {
            bool currentState = currentGeneration[cell];

            // Count alive neighbors using stack-allocated enumerator (zero allocation)
            int aliveNeighborCount = 0;
            foreach (HexPoint neighbor in Topology.GetNeighborsStack(cell))
            {
                if (currentGeneration[neighbor])
                {
                    aliveNeighborCount++;
                }
            }

            // Apply rules using count-based overload (zero allocation)
            if (_rules.GetNextState(currentState, aliveNeighborCount))
            {
                nextAlive.Add(cell);
            }
        }

        return new HexGeneration(nextAlive);
    }
}
