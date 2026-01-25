namespace GameOfLife.Core.Seeds;

public static class SeedPatternLoader
{
    public static SeedPattern LoadFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must be provided.", nameof(filePath));
        }

        var lines = File.ReadAllLines(filePath);
        return SeedPatternParser.Parse(lines);
    }
}
