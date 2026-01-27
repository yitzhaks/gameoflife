namespace GameOfLife.Core;

/// <summary>
/// Optimized world implementation specifically for 2D rectangular grids.
/// Uses array-based state storage and zero-allocation neighbor iteration for maximum performance.
/// </summary>
public class RectangularWorld : IWorld<Point2D, bool, IGeneration<Point2D, bool>>
{
    private readonly ICellularAutomatonRules _rules;

    /// <summary>
    /// Creates a new rectangular world with the specified size and classic rules.
    /// </summary>
    /// <param name="size">The size of the grid.</param>
    public RectangularWorld(Size2D size) : this(size, new ClassicRules())
    {
    }

    /// <summary>
    /// Creates a new rectangular world with the specified size and rules.
    /// </summary>
    /// <param name="size">The size of the grid.</param>
    /// <param name="rules">The cellular automata rules to apply.</param>
    public RectangularWorld(Size2D size, ICellularAutomatonRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        Size = size;
        Topology = new RectangularTopology(size);
        _rules = rules;
    }

    /// <summary>
    /// Gets the size of the grid.
    /// </summary>
    public Size2D Size { get; }

    /// <summary>
    /// Gets the topology.
    /// </summary>
    public RectangularTopology Topology { get; }

    /// <inheritdoc />
    ITopology<Point2D> IWorld<Point2D, bool, IGeneration<Point2D, bool>>.Topology => Topology;

    /// <inheritdoc />
    public IGeneration<Point2D, bool> Tick(IGeneration<Point2D, bool> currentGeneration)
    {
        ArgumentNullException.ThrowIfNull(currentGeneration);

        {
            using var builder = new RectangularGenerationBuilder(Size, clear: false);

            // Compute next state for each cell
            for (int y = 0; y < Size.Height; y++)
            {
                for (int x = 0; x < Size.Width; x++)
                {
                    Point2D cell = (x, y);
                    bool currentState = currentGeneration[cell];

                    // Count alive neighbors using stack-allocated enumerator (zero allocation)
                    int aliveNeighborCount = 0;
                    foreach (Point2D neighbor in Topology.GetNeighborsStack(cell))
                    {
                        if (currentGeneration[neighbor])
                        {
                            aliveNeighborCount++;
                        }
                    }

                    // Apply rules using count-based overload (zero allocation)
                    builder[cell] = _rules.GetNextState(currentState, aliveNeighborCount);
                }
            }

            // Build() takes ownership of the pooled array
            return builder.Build();
        }
    }
}
