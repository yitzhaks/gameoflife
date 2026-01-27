using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class PackedPoint2DTests
{
    [Theory]
    [InlineData(0, 0, 0, 0, true)]
    [InlineData(0, 1, 0, 0, false)]
    [InlineData(0, 2, 0, 1, true)]
    [InlineData(0, 3, 0, 1, false)]
    [InlineData(5, 4, 5, 2, true)]
    [InlineData(5, 5, 5, 2, false)]
    [InlineData(3, 10, 3, 5, true)]
    [InlineData(3, 11, 3, 5, false)]
    public void FromOriginal_MapsCorrectly(int origX, int origY, int expectedX, int expectedY, bool expectedTop)
    {
        Point2D original = (origX, origY);

        var packed = PackedPoint2D.FromOriginal(original);

        packed.X.ShouldBe(expectedX);
        packed.Y.ShouldBe(expectedY);
        packed.Top.ShouldBe(expectedTop);
    }

    [Theory]
    [InlineData(0, 0, true, 0, 0)]
    [InlineData(0, 0, false, 0, 1)]
    [InlineData(0, 1, true, 0, 2)]
    [InlineData(0, 1, false, 0, 3)]
    [InlineData(5, 2, true, 5, 4)]
    [InlineData(5, 2, false, 5, 5)]
    public void ToOriginal_MapsCorrectly(int packedX, int packedY, bool top, int expectedX, int expectedY)
    {
        var packed = new PackedPoint2D((packedX, packedY), top);

        Point2D original = packed.ToOriginal();

        original.X.ShouldBe(expectedX);
        original.Y.ShouldBe(expectedY);
    }

    [Fact]
    public void RoundTrip_PreservesValue()
    {
        Point2D original = (3, 7);

        var packed = PackedPoint2D.FromOriginal(original);
        Point2D restored = packed.ToOriginal();

        restored.ShouldBe(original);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        Point2D position = (2, 3);

        var packed = new PackedPoint2D(position, true);

        packed.Position.ShouldBe(position);
        packed.X.ShouldBe(2);
        packed.Y.ShouldBe(3);
        packed.Top.ShouldBeTrue();
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        var packed1 = new PackedPoint2D((1, 2), true);
        var packed2 = new PackedPoint2D((1, 2), true);

        packed1.ShouldBe(packed2);
        (packed1 == packed2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentTop_ReturnsFalse()
    {
        var packed1 = new PackedPoint2D((1, 2), true);
        var packed2 = new PackedPoint2D((1, 2), false);

        packed1.ShouldNotBe(packed2);
        (packed1 != packed2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_DifferentPosition_ReturnsFalse()
    {
        var packed1 = new PackedPoint2D((1, 2), true);
        var packed2 = new PackedPoint2D((1, 3), true);

        packed1.ShouldNotBe(packed2);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var packed1 = new PackedPoint2D((1, 2), true);
        var packed2 = new PackedPoint2D((1, 2), true);

        packed1.GetHashCode().ShouldBe(packed2.GetHashCode());
    }
}
