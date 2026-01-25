namespace GameOfLife.Core;

public interface ITopology<TIdentity> where TIdentity : notnull, IEquatable<TIdentity>
{
    IEnumerable<TIdentity> Nodes { get; }
    IEnumerable<TIdentity> GetNeighbors(TIdentity node);
}
