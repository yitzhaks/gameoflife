namespace GameOfLife.Rendering;

/// <summary>
/// Defines an axis-aligned bounding region with inclusive min and max corners.
/// </summary>
/// <typeparam name="TCoordinate">The coordinate type used to represent positions.</typeparam>
public interface IAxisAlignedBounds<TCoordinate> : IBounds<TCoordinate> where TCoordinate : struct
{
    /// <summary>
    /// Gets the minimum corner of the bounds (inclusive).
    /// </summary>
    TCoordinate Min { get; }

    /// <summary>
    /// Gets the maximum corner of the bounds (inclusive).
    /// </summary>
    TCoordinate Max { get; }
}
