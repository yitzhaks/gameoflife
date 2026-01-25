namespace GameOfLife.Core;

public interface IGeneration<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    TState this[TIdentity node] { get; }
}
