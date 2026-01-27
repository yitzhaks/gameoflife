namespace GameOfLife.Core;

/// <summary>
/// Defines the contract for a world that can compute generation ticks.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TState">The type representing cell state.</typeparam>
/// <typeparam name="TGeneration">The concrete generation type used by this world.</typeparam>
public interface IWorld<TIdentity, TState, TGeneration>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TGeneration : IGeneration<TIdentity, TState>
{
    /// <summary>
    /// Gets the topology defining the structure of the world.
    /// </summary>
    ITopology<TIdentity> Topology { get; }

    /// <summary>
    /// Computes the next generation by applying rules to each node.
    /// </summary>
    /// <param name="current">The current generation state.</param>
    /// <returns>A new generation representing the next state.</returns>
    TGeneration Tick(TGeneration current);
}
