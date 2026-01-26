using GameOfLife.Core;

namespace GameOfLife.Console;

/// <summary>
/// Loads shape patterns from text files.
/// </summary>
internal sealed class ShapeLoader
{
    private readonly string _patternsDirectory;

    /// <summary>
    /// Creates a new shape loader.
    /// </summary>
    /// <param name="patternsDirectory">The directory containing pattern files.</param>
    /// <exception cref="ArgumentNullException">Thrown if patternsDirectory is null.</exception>
    public ShapeLoader(string patternsDirectory)
    {
        ArgumentNullException.ThrowIfNull(patternsDirectory);
        _patternsDirectory = patternsDirectory;
    }

    /// <summary>
    /// Lists all available pattern names (filenames without extension).
    /// </summary>
    /// <returns>The available pattern names.</returns>
    public IEnumerable<string> GetAvailablePatterns()
    {
        if (!Directory.Exists(_patternsDirectory))
        {
            return [];
        }

        return Directory.GetFiles(_patternsDirectory, "*.txt")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name is not null)
            .Cast<string>()
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Loads a pattern by name (filename without extension).
    /// Returns a list of relative Point2D positions that are alive.
    /// </summary>
    /// <param name="name">The pattern name (filename without extension).</param>
    /// <returns>The alive cell positions relative to (0,0).</returns>
    /// <exception cref="ArgumentNullException">Thrown if name is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the pattern file doesn't exist.</exception>
    public IReadOnlyList<Point2D> LoadPattern(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var filePath = Path.Combine(_patternsDirectory, $"{name}.txt");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Pattern file not found: {filePath}", filePath);
        }

        var patternText = File.ReadAllText(filePath);
        return ParsePattern(patternText);
    }

    /// <summary>
    /// Parses pattern text into alive cell positions.
    /// '#' = alive, '.' or any other char = dead.
    /// </summary>
    /// <param name="patternText">The pattern text to parse.</param>
    /// <returns>The alive cell positions relative to (0,0).</returns>
    /// <exception cref="ArgumentNullException">Thrown if patternText is null.</exception>
    public static IReadOnlyList<Point2D> ParsePattern(string patternText)
    {
        ArgumentNullException.ThrowIfNull(patternText);

        var positions = new List<Point2D>();
        var lines = patternText.Split('\n');

        for (int y = 0; y < lines.Length; y++)
        {
            var line = lines[y].TrimEnd('\r');
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '#')
                {
                    positions.Add(new Point2D(x, y));
                }
            }
        }

        return positions;
    }
}
