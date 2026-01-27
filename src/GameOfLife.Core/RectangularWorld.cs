namespace GameOfLife.Core;

/// <summary>
/// Optimized world implementation specifically for 2D grids with classic rules.
/// Uses array-based state storage for maximum performance.
/// </summary>
public class RectangularWorld : IWorld<Point2D, bool, IGeneration<Point2D, bool>>
{
    private readonly ClassicRules _rules = new();

    /// <summary>
    /// Creates a new rectangular world with the specified size.
    /// </summary>
    /// <param name="size">The size of the grid.</param>
    public RectangularWorld(Size2D size)
    {
        Size = size;
        Topology = new RectangularTopology(size);
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
                    builder[(x, y)] = _rules.GetNextState(currentGeneration[(x, y)], currentGeneration.GetNeighborStates(Topology, (x, y)));
                }
            }

            // Build() takes ownership of the pooled array
            return builder.Build();
        }
    }
}
