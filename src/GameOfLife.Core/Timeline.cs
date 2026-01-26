namespace GameOfLife.Core;

/// <summary>
/// Holds a World and current state, enabling stepping through generations.
/// This is where state lives in the system.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TState">The type representing cell state.</typeparam>
public class Timeline<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    /// <summary>
    /// Gets the world (topology + rules) used for computing ticks.
    /// </summary>
    public World<TIdentity, TState> World { get; }

    /// <summary>
    /// Gets the current generation state.
    /// </summary>
    public IGeneration<TIdentity, TState> Current { get; private set; }

    /// <summary>
    /// Creates a new timeline with the specified world and initial state.
    /// </summary>
    /// <param name="world">The world defining topology and rules.</param>
    /// <param name="initial">The initial generation state.</param>
    public Timeline(World<TIdentity, TState> world, IGeneration<TIdentity, TState> initial)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        Current = initial ?? throw new ArgumentNullException(nameof(initial));
    }

    /// <summary>
    /// Advances the timeline by one generation.
    /// </summary>
    public void Step()
    {
        Current = World.Tick(Current);
    }

    /// <summary>
    /// Advances the timeline by the specified number of generations.
    /// If count is zero or negative, this is a no-op.
    /// </summary>
    /// <param name="count">The number of generations to advance.</param>
    public void Step(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Step();
        }
    }
}
