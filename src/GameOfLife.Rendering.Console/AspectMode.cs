namespace GameOfLife.Rendering.Console;

/// <summary>
/// Specifies the aspect ratio correction mode for console rendering.
/// </summary>
public enum AspectMode
{
    /// <summary>
    /// No aspect ratio correction. Each cell uses one character.
    /// </summary>
    None,

    /// <summary>
    /// Half-block vertical packing. Two vertically-adjacent cells are packed
    /// into a single character using Unicode half-block characters (▀ ▄ █).
    /// This produces a more square aspect ratio since console characters are
    /// typically taller than they are wide.
    /// </summary>
    HalfBlock
}
