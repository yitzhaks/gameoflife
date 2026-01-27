namespace GameOfLife.Rendering;

/// <summary>
/// Defines a region in coordinate space that can test containment.
/// </summary>
/// <typeparam name="TCoordinate">The coordinate type used to represent positions.</typeparam>
public interface IBounds<TCoordinate> where TCoordinate : struct
{
    /// <summary>
    /// Determines whether the specified coordinate is within the bounds.
    /// </summary>
    /// <param name="coordinate">The coordinate to test.</param>
    /// <returns><c>true</c> if the coordinate is within the bounds; otherwise, <c>false</c>.</returns>
    bool Contains(TCoordinate coordinate);
}
