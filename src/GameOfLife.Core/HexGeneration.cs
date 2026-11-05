namespace GameOfLife.Core;

/// <summary>
/// A sparse generation for hexagonal topologies using a HashSet for alive cells.
/// Dead cells don't require explicit storage, making this efficient for typical Game of Life patterns.
/// </summary>
public sealed class HexGeneration : IGeneration<HexPoint, bool>
{
    private readonly HashSet<HexPoint> _aliveCells;

    /// <summary>
    /// Creates a new hexagonal generation with no alive cells.
    /// </summary>
    public HexGeneration() : this([])
    {
    }

    /// <summary>
    /// Creates a new hexagonal generation with the specified alive cells.
    /// </summary>
    /// <param name="aliveCells">The cells that are alive. The collection is copied defensively.</param>
    public HexGeneration(IEnumerable<HexPoint> aliveCells)
    {
        ArgumentNullException.ThrowIfNull(aliveCells);

        // Defensive copy to ensure immutability
        _aliveCells = [.. aliveCells];
    }

    /// <summary>
    /// Gets the state of a cell. Returns true if alive, false if dead.
    /// </summary>
    public bool this[HexPoint node] => _aliveCells.Contains(node);

    /// <summary>
    /// Gets the alive cells in this generation.
    /// </summary>
    public IReadOnlySet<HexPoint> AliveCells => _aliveCells;

    /// <summary>
    /// No-op disposal. HexGeneration does not own unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // No unmanaged resources to dispose
    }
}
