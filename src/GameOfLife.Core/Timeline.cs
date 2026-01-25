namespace GameOfLife.Core;

public sealed class Timeline<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    public Timeline(World<TIdentity, TState> world, IGeneration<TIdentity, TState> initial)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        Current = initial ?? throw new ArgumentNullException(nameof(initial));
    }

    public World<TIdentity, TState> World { get; }
    public IGeneration<TIdentity, TState> Current { get; private set; }

    public void Step()
    {
        Current = World.Tick(Current);
    }

    public void Step(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");
        }

        for (var i = 0; i < count; i++)
        {
            Step();
        }
    }
}
