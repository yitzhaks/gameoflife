namespace GameOfLife.Core;

/// <summary>
/// Hex Life rules B2/S34 - optimized for 6-neighbor hex grids.
/// Birth: Dead cell with exactly 2 alive neighbors becomes alive.
/// Survival: Alive cell with 3 or 4 alive neighbors stays alive.
/// This is one of the most common hex life rule sets.
/// </summary>
public class HexRulesB2S34 : CellularAutomatonRules
{
    /// <inheritdoc />
    public override bool DefaultState => false;

    /// <inheritdoc />
    public override bool GetNextState(bool currentState, int aliveNeighborCount) =>
        currentState
            ? aliveNeighborCount is 3 or 4  // Survival
            : aliveNeighborCount is 2;       // Birth
}

/// <summary>
/// Hex Life rules B2/S35 - another popular hex variant.
/// Birth: Dead cell with exactly 2 alive neighbors becomes alive.
/// Survival: Alive cell with 3, 4, or 5 alive neighbors stays alive.
/// More permissive survival leads to larger stable structures.
/// </summary>
public class HexRulesB2S35 : CellularAutomatonRules
{
    /// <inheritdoc />
    public override bool DefaultState => false;

    /// <inheritdoc />
    public override bool GetNextState(bool currentState, int aliveNeighborCount) =>
        currentState
            ? aliveNeighborCount is >= 3 and <= 5  // Survival
            : aliveNeighborCount is 2;              // Birth
}

/// <summary>
/// Hex Life rules B24/S35 - balanced birth and survival.
/// Birth: Dead cell with 2 or 4 alive neighbors becomes alive.
/// Survival: Alive cell with 3, 4, or 5 alive neighbors stays alive.
/// </summary>
public class HexRulesB24S35 : CellularAutomatonRules
{
    /// <inheritdoc />
    public override bool DefaultState => false;

    /// <inheritdoc />
    public override bool GetNextState(bool currentState, int aliveNeighborCount) =>
        currentState
            ? aliveNeighborCount is >= 3 and <= 5  // Survival
            : aliveNeighborCount is 2 or 4;         // Birth
}

/// <summary>
/// Hex Life rules B2/S23 - similar structure to classic Conway.
/// Birth: Dead cell with exactly 2 alive neighbors becomes alive.
/// Survival: Alive cell with 2 or 3 alive neighbors stays alive.
/// Conservative variant that tends to die out without careful initial patterns.
/// </summary>
public class HexRulesB2S23 : CellularAutomatonRules
{
    /// <inheritdoc />
    public override bool DefaultState => false;

    /// <inheritdoc />
    public override bool GetNextState(bool currentState, int aliveNeighborCount) =>
        currentState
            ? aliveNeighborCount is 2 or 3  // Survival
            : aliveNeighborCount is 2;       // Birth
}
