using GameOfLife.Console;
using GameOfLife.Core;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public static class ShapeLoaderTests
{
    [Fact]
    public static void ParsePattern_GliderPattern_ReturnsCorrectPoints()
    {
        string pattern = ".#.\n..#\n###";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        points.Count.ShouldBe(5);
        points.ShouldContain((1, 0));
        points.ShouldContain((2, 1));
        points.ShouldContain((0, 2));
        points.ShouldContain((1, 2));
        points.ShouldContain((2, 2));
    }

    [Fact]
    public static void ParsePattern_BlockPattern_ReturnsCorrectPoints()
    {
        string pattern = "##\n##";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        points.Count.ShouldBe(4);
        points.ShouldContain((0, 0));
        points.ShouldContain((1, 0));
        points.ShouldContain((0, 1));
        points.ShouldContain((1, 1));
    }

    [Fact]
    public static void ParsePattern_BlinkerPattern_ReturnsCorrectPoints()
    {
        string pattern = "###";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        points.Count.ShouldBe(3);
        points.ShouldContain((0, 0));
        points.ShouldContain((1, 0));
        points.ShouldContain((2, 0));
    }

    [Fact]
    public static void ParsePattern_EmptyPattern_ReturnsEmptyList()
    {
        string pattern = "...\n...\n...";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        points.ShouldBeEmpty();
    }

    [Fact]
    public static void ParsePattern_SingleAliveCell_ReturnsSinglePoint()
    {
        string pattern = "#";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        _ = points.ShouldHaveSingleItem();
        points[0].ShouldBe((0, 0));
    }

    [Fact]
    public static void ParsePattern_WindowsLineEndings_ParsesCorrectly()
    {
        string pattern = ".#.\r\n..#\r\n###";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        points.Count.ShouldBe(5);
        points.ShouldContain((1, 0));
    }

    [Fact]
    public static void ParsePattern_MixedLineEndings_ParsesCorrectly()
    {
        string pattern = ".#.\r\n..#\n###";

        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(pattern);

        points.Count.ShouldBe(5);
    }

    [Fact]
    public static void ParsePattern_NullInput_ThrowsArgumentNullException() => _ = Should.Throw<ArgumentNullException>(() => ShapeLoader.ParsePattern(null!));

    [Fact]
    public static void ParsePattern_EmptyString_ReturnsEmptyList()
    {
        IReadOnlyList<Point2D> points = ShapeLoader.ParsePattern(string.Empty);

        points.ShouldBeEmpty();
    }

    [Fact]
    public static void Constructor_NullDirectory_ThrowsArgumentNullException() => _ = Should.Throw<ArgumentNullException>(() => new ShapeLoader(null!));

    [Fact]
    public static void GetAvailablePatterns_NonExistentDirectory_ReturnsEmptyEnumerable()
    {
        var loader = new ShapeLoader("nonexistent_directory_12345");

        IEnumerable<string> patterns = loader.GetAvailablePatterns();

        patterns.ShouldBeEmpty();
    }

    [Fact]
    public static void GetAvailablePatterns_DirectoryWithPatterns_ReturnsPatternNames()
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

            patterns.Count.ShouldBe(3);
            patterns.ShouldContain("blinker");
            patterns.ShouldContain("block");
            patterns.ShouldContain("glider");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public static void GetAvailablePatterns_DirectoryWithMixedFiles_ReturnsOnlyTxtFiles()
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

            _ = patterns.ShouldHaveSingleItem();
            patterns[0].ShouldBe("glider");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public static void LoadPattern_NullName_ThrowsArgumentNullException()
    {
        var loader = new ShapeLoader(".");

        _ = Should.Throw<ArgumentNullException>(() => loader.LoadPattern(null!));
    }

    [Fact]
    public static void LoadPattern_NonExistentPattern_ThrowsFileNotFoundException()
    {
        var loader = new ShapeLoader(".");

        _ = Should.Throw<FileNotFoundException>(() => loader.LoadPattern("nonexistent_pattern_12345"));
    }

    [Fact]
    public static void LoadPattern_ExistingPattern_ReturnsPoints()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gol_test_{Guid.NewGuid()}");
        _ = Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "glider.txt"), ".#.\n..#\n###");

            var loader = new ShapeLoader(tempDir);
            IReadOnlyList<Point2D> points = loader.LoadPattern("glider");

            points.Count.ShouldBe(5);
            points.ShouldContain((1, 0));
            points.ShouldContain((2, 1));
            points.ShouldContain((0, 2));
            points.ShouldContain((1, 2));
            points.ShouldContain((2, 2));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
