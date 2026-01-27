using GameOfLife.Console;
using GameOfLife.Core;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ShapeLoaderTests
{
    [Fact]
    public void ParsePattern_GliderPattern_ReturnsCorrectPoints()
    {
        string pattern = ".#.\n..#\n###";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(5, points.Count);
        Assert.Contains((1, 0), points);
        Assert.Contains((2, 1), points);
        Assert.Contains((0, 2), points);
        Assert.Contains((1, 2), points);
        Assert.Contains((2, 2), points);
    }

    [Fact]
    public void ParsePattern_BlockPattern_ReturnsCorrectPoints()
    {
        string pattern = "##\n##";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(4, points.Count);
        Assert.Contains(default, points);
        Assert.Contains((1, 0), points);
        Assert.Contains((0, 1), points);
        Assert.Contains((1, 1), points);
    }

    [Fact]
    public void ParsePattern_BlinkerPattern_ReturnsCorrectPoints()
    {
        string pattern = "###";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(3, points.Count);
        Assert.Contains(default, points);
        Assert.Contains((1, 0), points);
        Assert.Contains((2, 0), points);
    }

    [Fact]
    public void ParsePattern_EmptyPattern_ReturnsEmptyList()
    {
        string pattern = "...\n...\n...";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        Assert.Empty(points);
    }

    [Fact]
    public void ParsePattern_SingleAliveCell_ReturnsSinglePoint()
    {
        string pattern = "#";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        _ = Assert.Single(points);
        Assert.Equal(default, points[0]);
    }

    [Fact]
    public void ParsePattern_WindowsLineEndings_ParsesCorrectly()
    {
        string pattern = ".#.\r\n..#\r\n###";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(5, points.Count);
        Assert.Contains((1, 0), points);
    }

    [Fact]
    public void ParsePattern_MixedLineEndings_ParsesCorrectly()
    {
        string pattern = ".#.\r\n..#\n###";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(5, points.Count);
    }

    [Fact]
    public void ParsePattern_NullInput_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(() => ShapeLoader.ParsePattern(null!));

    [Fact]
    public void ParsePattern_EmptyString_ReturnsEmptyList()
    {
        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(string.Empty);

        Assert.Empty(points);
    }

    [Fact]
    public void Constructor_NullDirectory_ThrowsArgumentNullException() => Assert.Throws<ArgumentNullException>(() => new ShapeLoader(null!));

    [Fact]
    public void GetAvailablePatterns_NonExistentDirectory_ReturnsEmptyEnumerable()
    {
        var loader = new ShapeLoader("nonexistent_directory_12345");

        IEnumerable<string> patterns = loader.GetAvailablePatterns();

        Assert.Empty(patterns);
    }

    [Fact]
    public void GetAvailablePatterns_DirectoryWithPatterns_ReturnsPatternNames()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "glider.txt"), ".#.\n..#\n###");
            File.WriteAllText(Path.Combine(tempDir, "block.txt"), "##\n##");
            File.WriteAllText(Path.Combine(tempDir, "blinker.txt"), "###");

            var loader = new ShapeLoader(tempDir);
            var patterns = loader.GetAvailablePatterns().ToList();

            Assert.Equal(3, patterns.Count);
            Assert.Contains("blinker", patterns);
            Assert.Contains("block", patterns);
            Assert.Contains("glider", patterns);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void GetAvailablePatterns_DirectoryWithMixedFiles_ReturnsOnlyTxtFiles()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "glider.txt"), ".#.\n..#\n###");
            File.WriteAllText(Path.Combine(tempDir, "readme.md"), "# Patterns");
            File.WriteAllText(Path.Combine(tempDir, "data.json"), "{}");

            var loader = new ShapeLoader(tempDir);
            var patterns = loader.GetAvailablePatterns().ToList();

            _ = Assert.Single(patterns);
            Assert.Equal("glider", patterns[0]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void LoadPattern_NullName_ThrowsArgumentNullException()
    {
        var loader = new ShapeLoader(".");

        _ = Assert.Throws<ArgumentNullException>(() => loader.LoadPattern(null!));
    }

    [Fact]
    public void LoadPattern_NonExistentPattern_ThrowsFileNotFoundException()
    {
        var loader = new ShapeLoader(".");

        _ = Assert.Throws<FileNotFoundException>(() => loader.LoadPattern("nonexistent_pattern_12345"));
    }

    [Fact]
    public void LoadPattern_ExistingPattern_ReturnsPoints()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "glider.txt"), ".#.\n..#\n###");

            var loader = new ShapeLoader(tempDir);
            IReadOnlyList<Point2D> points = loader.LoadPattern("glider");

            Assert.Equal(5, points.Count);
            Assert.Contains((1, 0), points);
            Assert.Contains((2, 1), points);
            Assert.Contains((0, 2), points);
            Assert.Contains((1, 2), points);
            Assert.Contains((2, 2), points);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
