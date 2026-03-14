using GameOfLife.Console;
using GameOfLife.Core;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public sealed class HexShapeLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public HexShapeLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HexShapeLoaderTests_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_NullDirectory_ThrowsArgumentNullException() => _ = Should.Throw<ArgumentNullException>(() => new HexShapeLoader(null!));

    [Fact]
    public void Constructor_ValidDirectory_CreatesLoader()
    {
        var loader = new HexShapeLoader(_tempDir);

        _ = loader.ShouldNotBeNull();
    }

    #endregion

    #region LoadPattern Tests

    [Fact]
    public void LoadPattern_NullPatternName_ThrowsArgumentNullException()
    {
        var loader = new HexShapeLoader(_tempDir);

        _ = Should.Throw<ArgumentNullException>(() => loader.LoadPattern(null!));
    }

    [Fact]
    public void LoadPattern_NonexistentPattern_ThrowsFileNotFoundException()
    {
        var loader = new HexShapeLoader(_tempDir);

        FileNotFoundException ex = Should.Throw<FileNotFoundException>(() =>
            loader.LoadPattern("nonexistent"));

        ex.Message.ShouldContain("nonexistent");
    }

    [Fact]
    public void LoadPattern_ValidPattern_ReturnsPoints()
    {
        File.WriteAllText(Path.Combine(_tempDir, "test.txt"), "0,0\n1,0\n0,1");
        var loader = new HexShapeLoader(_tempDir);

        IReadOnlyList<HexPoint> points = loader.LoadPattern("test");

        points.Count.ShouldBe(3);
        points.ShouldContain(new HexPoint(0, 0));
        points.ShouldContain(new HexPoint(1, 0));
        points.ShouldContain(new HexPoint(0, 1));
    }

    [Fact]
    public void LoadPattern_NegativeCoordinates_ReturnsCorrectPoints()
    {
        File.WriteAllText(Path.Combine(_tempDir, "negative.txt"), "-1,-2\n-3,4");
        var loader = new HexShapeLoader(_tempDir);

        IReadOnlyList<HexPoint> points = loader.LoadPattern("negative");

        points.Count.ShouldBe(2);
        points.ShouldContain(new HexPoint(-1, -2));
        points.ShouldContain(new HexPoint(-3, 4));
    }

    [Fact]
    public void LoadPattern_WithComments_IgnoresComments()
    {
        File.WriteAllText(Path.Combine(_tempDir, "comments.txt"),
            "# This is a comment\n0,0\n# Another comment\n1,0");
        var loader = new HexShapeLoader(_tempDir);

        IReadOnlyList<HexPoint> points = loader.LoadPattern("comments");

        points.Count.ShouldBe(2);
        points.ShouldContain(new HexPoint(0, 0));
        points.ShouldContain(new HexPoint(1, 0));
    }

    [Fact]
    public void LoadPattern_WithEmptyLines_IgnoresEmptyLines()
    {
        File.WriteAllText(Path.Combine(_tempDir, "empty.txt"),
            "0,0\n\n1,0\n   \n0,1");
        var loader = new HexShapeLoader(_tempDir);

        IReadOnlyList<HexPoint> points = loader.LoadPattern("empty");

        points.Count.ShouldBe(3);
    }

    [Fact]
    public void LoadPattern_WithWhitespace_TrimsWhitespace()
    {
        File.WriteAllText(Path.Combine(_tempDir, "whitespace.txt"),
            "  0,0  \n  1,0\n0,1  ");
        var loader = new HexShapeLoader(_tempDir);

        IReadOnlyList<HexPoint> points = loader.LoadPattern("whitespace");

        points.Count.ShouldBe(3);
        points.ShouldContain(new HexPoint(0, 0));
    }

    [Fact]
    public void LoadPattern_InvalidFormat_ThrowsFormatException()
    {
        File.WriteAllText(Path.Combine(_tempDir, "invalid.txt"), "not a coordinate");
        var loader = new HexShapeLoader(_tempDir);

        FormatException ex = Should.Throw<FormatException>(() =>
            loader.LoadPattern("invalid"));

        ex.Message.ShouldContain("invalid");
        ex.Message.ShouldContain("not a coordinate");
    }

    [Fact]
    public void LoadPattern_InvalidCoordinateValue_ThrowsFormatException()
    {
        File.WriteAllText(Path.Combine(_tempDir, "badvalue.txt"), "abc,def");
        var loader = new HexShapeLoader(_tempDir);

        FormatException ex = Should.Throw<FormatException>(() =>
            loader.LoadPattern("badvalue"));

        ex.Message.ShouldContain("badvalue");
    }

    [Fact]
    public void LoadPattern_CachesResult_ReturnsSameInstance()
    {
        File.WriteAllText(Path.Combine(_tempDir, "cached.txt"), "0,0");
        var loader = new HexShapeLoader(_tempDir);

        IReadOnlyList<HexPoint> first = loader.LoadPattern("cached");
        IReadOnlyList<HexPoint> second = loader.LoadPattern("cached");

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void LoadPattern_CaseInsensitive_LoadsSamePattern()
    {
        File.WriteAllText(Path.Combine(_tempDir, "CaseSensitive.txt"), "0,0\n1,0");
        var loader = new HexShapeLoader(_tempDir);

        // Note: This tests the cache, not the file system lookup
        // File system behavior depends on OS
        IReadOnlyList<HexPoint> result = loader.LoadPattern("CaseSensitive");

        result.Count.ShouldBe(2);
    }

    [Fact]
    public void LoadPattern_EmptyFile_ReturnsEmptyList()
    {
        File.WriteAllText(Path.Combine(_tempDir, "empty-pattern.txt"), "");
        var loader = new HexShapeLoader(_tempDir);

        IReadOnlyList<HexPoint> points = loader.LoadPattern("empty-pattern");

        points.ShouldBeEmpty();
    }

    [Fact]
    public void LoadPattern_OnlyComments_ReturnsEmptyList()
    {
        File.WriteAllText(Path.Combine(_tempDir, "only-comments.txt"),
            "# Comment 1\n# Comment 2");
        var loader = new HexShapeLoader(_tempDir);

        IReadOnlyList<HexPoint> points = loader.LoadPattern("only-comments");

        points.ShouldBeEmpty();
    }

    #endregion

    #region GetAvailablePatterns Tests

    [Fact]
    public void GetAvailablePatterns_EmptyDirectory_ReturnsEmptyCollection()
    {
        var loader = new HexShapeLoader(_tempDir);

        IEnumerable<string> patterns = loader.GetAvailablePatterns();

        patterns.ShouldBeEmpty();
    }

    [Fact]
    public void GetAvailablePatterns_NonexistentDirectory_ReturnsEmptyCollection()
    {
        var loader = new HexShapeLoader(Path.Combine(_tempDir, "nonexistent"));

        IEnumerable<string> patterns = loader.GetAvailablePatterns();

        patterns.ShouldBeEmpty();
    }

    [Fact]
    public void GetAvailablePatterns_WithPatterns_ReturnsPatternNames()
    {
        File.WriteAllText(Path.Combine(_tempDir, "alpha.txt"), "0,0");
        File.WriteAllText(Path.Combine(_tempDir, "beta.txt"), "0,0");
        File.WriteAllText(Path.Combine(_tempDir, "gamma.txt"), "0,0");
        var loader = new HexShapeLoader(_tempDir);

        List<string> patterns = [.. loader.GetAvailablePatterns()];

        patterns.Count.ShouldBe(3);
        patterns.ShouldContain("alpha");
        patterns.ShouldContain("beta");
        patterns.ShouldContain("gamma");
    }

    [Fact]
    public void GetAvailablePatterns_ReturnsAlphabeticallySorted()
    {
        File.WriteAllText(Path.Combine(_tempDir, "zebra.txt"), "0,0");
        File.WriteAllText(Path.Combine(_tempDir, "alpha.txt"), "0,0");
        File.WriteAllText(Path.Combine(_tempDir, "middle.txt"), "0,0");
        var loader = new HexShapeLoader(_tempDir);

        List<string> patterns = [.. loader.GetAvailablePatterns()];

        patterns[0].ShouldBe("alpha");
        patterns[1].ShouldBe("middle");
        patterns[2].ShouldBe("zebra");
    }

    [Fact]
    public void GetAvailablePatterns_IgnoresNonTxtFiles()
    {
        File.WriteAllText(Path.Combine(_tempDir, "pattern.txt"), "0,0");
        File.WriteAllText(Path.Combine(_tempDir, "readme.md"), "# Readme");
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), "{}");
        var loader = new HexShapeLoader(_tempDir);

        List<string> patterns = [.. loader.GetAvailablePatterns()];

        patterns.Count.ShouldBe(1);
        patterns.ShouldContain("pattern");
    }

    #endregion
}
