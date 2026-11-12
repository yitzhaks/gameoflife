namespace GameOfLife.Core;

/// <summary>
/// Represents a hexagonal coordinate using axial coordinates (Q, R).
/// Uses the "pointy-top" orientation with the cube coordinate constraint q + r + s = 0.
/// </summary>
/// <param name="Q">The Q coordinate (column axis).</param>
/// <param name="R">The R coordinate (row axis).</param>
public readonly record struct HexPoint(int Q, int R)
{
    /// <summary>
    /// Gets the derived S cube coordinate (q + r + s = 0, so s = -q - r).
    /// </summary>
    public int S => -Q - R;

    /// <summary>
    /// Implicitly converts a tuple to a HexPoint.
    /// </summary>
    /// <param name="tuple">The tuple containing Q and R coordinates.</param>
    public static implicit operator HexPoint((int Q, int R) tuple) => new(tuple.Q, tuple.R);

    /// <summary>
    /// Adds two hex points component-wise.
    /// </summary>
    public static HexPoint operator +(HexPoint a, HexPoint b) => new(a.Q + b.Q, a.R + b.R);

    /// <summary>
    /// Subtracts the second hex point from the first component-wise.
    /// </summary>
    public static HexPoint operator -(HexPoint a, HexPoint b) => new(a.Q - b.Q, a.R - b.R);

    /// <summary>
    /// Calculates the hex Manhattan distance to another point using cube coordinates.
    /// </summary>
    /// <param name="other">The other hex point.</param>
    /// <returns>The hex distance (minimum number of hex steps).</returns>
    public int DistanceTo(HexPoint other)
    {
        int dq = Math.Abs(Q - other.Q);
        int dr = Math.Abs(R - other.R);
        int ds = Math.Abs(S - other.S);
        return (dq + dr + ds) / 2;
    }

    /// <summary>
    /// Checks if this point is within a given radius of the origin (0, 0).
    /// Uses the hex distance metric: max(|q|, |r|, |s|) &lt;= radius.
    /// </summary>
    /// <param name="radius">The radius to check against.</param>
    /// <returns>True if the point is within the radius; otherwise, false.</returns>
    public bool IsWithinRadius(int radius)
    {
        int absQ = Math.Abs(Q);
        int absR = Math.Abs(R);
        int absS = Math.Abs(S);
        return absQ <= radius && absR <= radius && absS <= radius;
    }
}
