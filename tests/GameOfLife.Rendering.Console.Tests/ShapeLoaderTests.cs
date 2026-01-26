using GameOfLife.Console;
using GameOfLife.Core;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ShapeLoaderTests
{
    [Fact]
    public void ParsePattern_GliderPattern_ReturnsCorrectPoints()
    {
        var pattern = ".#.\n..#\n###";

        var points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(5, points.Count);
        Assert.Contains(new Point2D(1, 0), points);
        Assert.Contains(new Point2D(2, 1), points);
        Assert.Contains(new Point2D(0, 2), points);
        Assert.Contains(new Point2D(1, 2), points);
        Assert.Contains(new Point2D(2, 2), points);
    }

    [Fact]
    public void ParsePattern_BlockPattern_ReturnsCorrectPoints()
    {
        var pattern = "##\n##";

        var points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(4, points.Count);
        Assert.Contains(new Point2D(0, 0), points);
        Assert.Contains(new Point2D(1, 0), points);
        Assert.Contains(new Point2D(0, 1), points);
        Assert.Contains(new Point2D(1, 1), points);
    }

    [Fact]
    public void ParsePattern_BlinkerPattern_ReturnsCorrectPoints()
    {
        var pattern = "###";

        var points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(3, points.Count);
        Assert.Contains(new Point2D(0, 0), points);
        Assert.Contains(new Point2D(1, 0), points);
        Assert.Contains(new Point2D(2, 0), points);
    }

    [Fact]
    public void ParsePattern_EmptyPattern_ReturnsEmptyList()
    {
        var pattern = "...\n...\n...";

        var points = ShapeLoader.ParsePattern(pattern);

        Assert.Empty(points);
    }

    [Fact]
    public void ParsePattern_SingleAliveCell_ReturnsSinglePoint()
    {
        var pattern = "#";

        var points = ShapeLoader.ParsePattern(pattern);

        Assert.Single(points);
        Assert.Equal(new Point2D(0, 0), points[0]);
    }

    [Fact]
    public void ParsePattern_WindowsLineEndings_ParsesCorrectly()
    {
        var pattern = ".#.\r\n..#\r\n###";

        var points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(5, points.Count);
        Assert.Contains(new Point2D(1, 0), points);
    }

    [Fact]
    public void ParsePattern_MixedLineEndings_ParsesCorrectly()
    {
        var pattern = ".#.\r\n..#\n###";

        var points = ShapeLoader.ParsePattern(pattern);

        Assert.Equal(5, points.Count);
    }

    [Fact]
    public void ParsePattern_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ShapeLoader.ParsePattern(null!));
    }

    [Fact]
    public void ParsePattern_EmptyString_ReturnsEmptyList()
    {
        var points = ShapeLoader.ParsePattern(string.Empty);

        Assert.Empty(points);
    }

    [Fact]
    public void Constructor_NullDirectory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ShapeLoader(null!));
    }

    [Fact]
    public void GetAvailablePatterns_NonExistentDirectory_ReturnsEmptyEnumerable()
    {
        var loader = new ShapeLoader("nonexistent_directory_12345");

        var patterns = loader.GetAvailablePatterns();

        Assert.Empty(patterns);
    }

    [Fact]
    public void GetAvailablePatterns_DirectoryWithPatterns_ReturnsPatternNames()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
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
        var tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "glider.txt"), ".#.\n..#\n###");
            File.WriteAllText(Path.Combine(tempDir, "readme.md"), "# Patterns");
            File.WriteAllText(Path.Combine(tempDir, "data.json"), "{}");

            var loader = new ShapeLoader(tempDir);
            var patterns = loader.GetAvailablePatterns().ToList();

            Assert.Single(patterns);
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

        Assert.Throws<ArgumentNullException>(() => loader.LoadPattern(null!));
    }

    [Fact]
    public void LoadPattern_NonExistentPattern_ThrowsFileNotFoundException()
    {
        var loader = new ShapeLoader(".");

        Assert.Throws<FileNotFoundException>(() => loader.LoadPattern("nonexistent_pattern_12345"));
    }

    [Fact]
    public void LoadPattern_ExistingPattern_ReturnsPoints()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "glider.txt"), ".#.\n..#\n###");

            var loader = new ShapeLoader(tempDir);
            var points = loader.LoadPattern("glider");

            Assert.Equal(5, points.Count);
            Assert.Contains(new Point2D(1, 0), points);
            Assert.Contains(new Point2D(2, 1), points);
            Assert.Contains(new Point2D(0, 2), points);
            Assert.Contains(new Point2D(1, 2), points);
            Assert.Contains(new Point2D(2, 2), points);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
