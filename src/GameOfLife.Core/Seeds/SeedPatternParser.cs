using GameOfLife.Core;

namespace GameOfLife.Core.Seeds;

public static class SeedPatternParser
{
    public static SeedPattern Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var materializedLines = lines.ToList();
        if (materializedLines.Count == 0)
        {
            throw new ArgumentException("Seed pattern must contain at least one row.", nameof(lines));
        }

        var width = materializedLines[0].Length;
        if (width == 0)
        {
            throw new ArgumentException("Seed pattern rows must not be empty.", nameof(lines));
        }

        var height = materializedLines.Count;
        var alivePoints = new List<Point2D>();

        for (var y = 0; y < height; y++)
        {
            var line = materializedLines[y];
            if (line.Length != width)
            {
                throw new ArgumentException("Seed pattern rows must all be the same length.", nameof(lines));
            }

            for (var x = 0; x < line.Length; x++)
            {
                var character = line[x];
                if (character == '#')
                {
                    alivePoints.Add(new Point2D(x, y));
                }
                else if (character != '.')
                {
                    throw new ArgumentException("Seed pattern contains invalid characters.", nameof(lines));
                }
            }
        }

        return new SeedPattern(width, height, alivePoints);
    }
}
