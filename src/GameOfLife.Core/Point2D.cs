namespace GameOfLife.Core;

public readonly struct Point2D(int x, int y) : IEquatable<Point2D>
{
    public int X { get; } = x;
    public int Y { get; } = y;

    public bool Equals(Point2D other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is Point2D other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);

    public static bool operator !=(Point2D left, Point2D right) => !left.Equals(right);
}
