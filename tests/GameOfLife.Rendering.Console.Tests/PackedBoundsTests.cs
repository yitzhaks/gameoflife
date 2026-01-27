using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class PackedBoundsTests
{
    [Fact]
    public void Constructor_EvenHeight_Succeeds()
    {
        var bounds = new PackedBounds(10, 8);

        bounds.Width.ShouldBe(10);
        bounds.OriginalHeight.ShouldBe(8);
        bounds.Height.ShouldBe(4);
    }

    [Fact]
    public void Constructor_ZeroHeight_Succeeds()
    {
        var bounds = new PackedBounds(10, 0);

        bounds.Width.ShouldBe(10);
        bounds.OriginalHeight.ShouldBe(0);
        bounds.Height.ShouldBe(0);
    }

    [Fact]
    public void Constructor_ZeroWidth_Succeeds()
    {
        var bounds = new PackedBounds(0, 4);

        bounds.Width.ShouldBe(0);
        bounds.Height.ShouldBe(2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(99)]
    public void Constructor_OddHeight_ThrowsArgumentException(int height)
    {
        ArgumentException exception = Should.Throw<ArgumentException>(() => new PackedBounds(10, height));

        exception.Message.ShouldContain("even");
        exception.Message.ShouldContain(height.ToString());
    }

    [Fact]
    public void Constructor_NegativeHeight_ThrowsArgumentOutOfRangeException() => _ = Should.Throw<ArgumentOutOfRangeException>(() => new PackedBounds(10, -2));

    [Fact]
    public void Min_NonEmptyBounds_ReturnsOrigin()
    {
        var bounds = new PackedBounds(10, 8);

        bounds.Min.X.ShouldBe(0);
        bounds.Min.Y.ShouldBe(0);
        bounds.Min.Top.ShouldBeTrue();
    }

    [Fact]
    public void Max_NonEmptyBounds_ReturnsCorrectCorner()
    {
        var bounds = new PackedBounds(10, 8);

        bounds.Max.X.ShouldBe(9);
        bounds.Max.Y.ShouldBe(3);
        bounds.Max.Top.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(5, 2, true)]
    [InlineData(9, 3, true)]
    [InlineData(0, 0, false)]
    public void Contains_ValidCoordinate_ReturnsTrue(int x, int y, bool top)
    {
        var bounds = new PackedBounds(10, 8);
        var packed = new PackedPoint2D((x, y), top);

        bounds.Contains(packed).ShouldBeTrue();
    }

    [Theory]
    [InlineData(-1, 0, true)]
    [InlineData(0, -1, true)]
    [InlineData(10, 0, true)]
    [InlineData(0, 4, true)]
    public void Contains_OutOfBoundsCoordinate_ReturnsFalse(int x, int y, bool top)
    {
        var bounds = new PackedBounds(10, 8);
        var packed = new PackedPoint2D((x, y), top);

        bounds.Contains(packed).ShouldBeFalse();
    }

    [Fact]
    public void Contains_EmptyBounds_ReturnsFalse()
    {
        var bounds = new PackedBounds(0, 0);
        var packed = new PackedPoint2D((0, 0), true);

        bounds.Contains(packed).ShouldBeFalse();
    }

    [Fact]
    public void Contains_ZeroWidthBounds_ReturnsFalse()
    {
        var bounds = new PackedBounds(0, 4);
        var packed = new PackedPoint2D((0, 0), true);

        bounds.Contains(packed).ShouldBeFalse();
    }
}
