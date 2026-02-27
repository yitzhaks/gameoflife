using GameOfLife.Core;

namespace GameOfLife.Console;

/// <summary>
/// Loads hex patterns from embedded resources.
/// </summary>
internal sealed class HexShapeLoader
{
    private readonly Dictionary<string, IReadOnlyList<HexPoint>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _patternsDirectory;

    /// <summary>
    /// Creates a new hex shape loader.
    /// </summary>
    /// <param name="patternsDirectory">The directory containing pattern files.</param>
    public HexShapeLoader(string patternsDirectory)
    {
        ArgumentNullException.ThrowIfNull(patternsDirectory);
        _patternsDirectory = patternsDirectory;
    }

    /// <summary>
    /// Loads a hex pattern by name.
    /// </summary>
    /// <param name="patternName">The pattern name (without extension).</param>
    /// <returns>List of HexPoint offsets relative to origin.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the pattern file doesn't exist.</exception>
    public IReadOnlyList<HexPoint> LoadPattern(string patternName)
    {
        ArgumentNullException.ThrowIfNull(patternName);

        if (_cache.TryGetValue(patternName, out IReadOnlyList<HexPoint>? cached))
        {
            return cached;
        }

        string filePath = Path.Combine(_patternsDirectory, $"{patternName}.txt");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Hex pattern file not found: {patternName}", filePath);
        }

        var points = new List<HexPoint>();
        foreach (string trimmed in File.ReadLines(filePath).Select(line => line.Trim()))
        {
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            // Parse Q,R coordinate
            int commaIndex = trimmed.IndexOf(',', StringComparison.Ordinal);
            if (commaIndex < 0)
            {
                throw new FormatException($"Invalid coordinate format in pattern '{patternName}': {trimmed}");
            }

            string qPart = trimmed[..commaIndex];
            string rPart = trimmed[(commaIndex + 1)..];

            if (!int.TryParse(qPart, out int q) || !int.TryParse(rPart, out int r))
            {
                throw new FormatException($"Invalid coordinate values in pattern '{patternName}': {trimmed}");
            }

            points.Add((q, r));
        }

        _cache[patternName] = points;
        return points;
    }

    /// <summary>
    /// Gets the names of all available hex patterns.
    /// </summary>
    /// <returns>Collection of pattern names.</returns>
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
}
