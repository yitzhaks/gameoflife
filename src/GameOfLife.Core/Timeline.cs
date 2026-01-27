namespace GameOfLife.Core;

/// <summary>
/// Factory methods for creating Timeline instances with type inference.
/// </summary>
public static class Timeline
{
    /// <summary>
    /// Creates a new timeline with the specified world and initial state.
    /// </summary>
    /// <param name="world">The world defining topology and rules.</param>
    /// <param name="initial">The initial generation state.</param>
    public static Timeline<TIdentity, TState, TGeneration> Create<TIdentity, TState, TGeneration>(
        IWorld<TIdentity, TState, TGeneration> world,
        TGeneration initial)
        where TIdentity : notnull, IEquatable<TIdentity>
        where TGeneration : IGeneration<TIdentity, TState> =>
        new(world, initial);
}

/// <summary>
/// Holds a World and current state, enabling stepping through generations.
/// This is where state lives in the system.
/// </summary>
/// <typeparam name="TIdentity">The type used to identify nodes.</typeparam>
/// <typeparam name="TState">The type representing cell state.</typeparam>
/// <typeparam name="TGeneration">The concrete generation type.</typeparam>
public sealed class Timeline<TIdentity, TState, TGeneration> : IDisposable
    where TIdentity : notnull, IEquatable<TIdentity>
    where TGeneration : IGeneration<TIdentity, TState>
{
    private TGeneration? _current;
    private bool _disposed;

    /// <summary>
    /// Gets the world (topology + rules) used for computing ticks.
    /// </summary>
    public IWorld<TIdentity, TState, TGeneration> World { get; }

    /// <summary>
    /// Gets the current generation state.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when accessed after disposal.</exception>
    public TGeneration Current
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _current!;
        }
        private set
        {
            ArgumentNullException.ThrowIfNull(value);
            _current = value;
        }
    }

    /// <summary>
    /// Creates a new timeline with the specified world and initial state.
    /// </summary>
    /// <param name="world">The world defining topology and rules.</param>
    /// <param name="initial">The initial generation state.</param>
    public Timeline(IWorld<TIdentity, TState, TGeneration> world, TGeneration initial)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        Current = initial; // Current already validates not null
    }

    /// <summary>
    /// Advances the timeline by one generation.
    /// Disposes the previous generation.
    /// </summary>
    public void Step()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using TGeneration previous = Current;
        Current = World.Tick(previous);
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

    /// <summary>
    /// Disposes the current generation.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _current?.Dispose();
        _current = default;
        _disposed = true;
    }
}
