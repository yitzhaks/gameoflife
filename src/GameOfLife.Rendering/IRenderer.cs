using GameOfLife.Core;

namespace GameOfLife.Rendering;

/// <summary>
/// Renders generation state to a specific output format.
/// Renderers use a layout engine to build geometry and convert state into visual output.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TCoordinate">The coordinate type used to represent positions.</typeparam>
/// <typeparam name="TBounds">The bounds type for the layout region.</typeparam>
/// <typeparam name="TState">The type representing the state of each node.</typeparam>
public interface IRenderer<TIdentity, TCoordinate, TBounds, TState>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
    where TBounds : IBounds<TCoordinate>
{
    /// <summary>
    /// Renders the generation state for the specified topology.
    /// </summary>
    /// <param name="topology">The topology defining the structure.</param>
    /// <param name="generation">The generation state to render.</param>
    void Render(ITopology<TIdentity> topology, IGeneration<TIdentity, TState> generation);
}
