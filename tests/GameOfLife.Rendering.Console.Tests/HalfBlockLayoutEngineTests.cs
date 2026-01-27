using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class HalfBlockLayoutEngineTests
{
    [Fact]
    public void CreateLayout_ValidTopology_ReturnsLayout()
    {
        var engine = new HalfBlockLayoutEngine();
        var topology = new RectangularTopology((5, 6));

        ILayout<Point2D, PackedPoint2D, PackedBounds> layout = engine.CreateLayout(topology);

        _ = layout.ShouldNotBeNull();
        _ = layout.Positions.ShouldNotBeNull();
        _ = layout.Bounds.ShouldNotBeNull();
    }

    [Fact]
    public void CreateLayout_NullTopology_ThrowsArgumentNullException()
    {
        var engine = new HalfBlockLayoutEngine();

        _ = Should.Throw<ArgumentNullException>(() => engine.CreateLayout(null!));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void CreateLayout_OddHeight_ThrowsArgumentException(int height)
    {
        var engine = new HalfBlockLayoutEngine();
        var topology = new RectangularTopology((4, height));

        _ = Should.Throw<ArgumentException>(() => engine.CreateLayout(topology));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(100)]
    public void CreateLayout_EvenHeight_Succeeds(int height)
    {
        var engine = new HalfBlockLayoutEngine();
        var topology = new RectangularTopology((4, height));

        ILayout<Point2D, PackedPoint2D, PackedBounds> layout = engine.CreateLayout(topology);

        layout.Bounds.Height.ShouldBe(height / 2);
    }

    [Fact]
    public void CreateLayout_3x4Grid_BoundsAreCorrect()
    {
        var engine = new HalfBlockLayoutEngine();
        var topology = new RectangularTopology((3, 4));

        ILayout<Point2D, PackedPoint2D, PackedBounds> layout = engine.CreateLayout(topology);

        layout.Bounds.Width.ShouldBe(3);
        layout.Bounds.Height.ShouldBe(2);
        layout.Bounds.Min.X.ShouldBe(0);
        layout.Bounds.Min.Y.ShouldBe(0);
        layout.Bounds.Max.X.ShouldBe(2);
        layout.Bounds.Max.Y.ShouldBe(1);
    }

    [Fact]
    public void CreateLayout_PositionsMapsCorrectly()
    {
        var engine = new HalfBlockLayoutEngine();
        var topology = new RectangularTopology((3, 4));

        ILayout<Point2D, PackedPoint2D, PackedBounds> layout = engine.CreateLayout(topology);

        // Test various positions
        PackedPoint2D pos = layout.Positions[(1, 0)];
        pos.X.ShouldBe(1);
        pos.Y.ShouldBe(0);
        pos.Top.ShouldBeTrue();

        pos = layout.Positions[(1, 1)];
        pos.X.ShouldBe(1);
        pos.Y.ShouldBe(0);
        pos.Top.ShouldBeFalse();

        pos = layout.Positions[(2, 3)];
        pos.X.ShouldBe(2);
        pos.Y.ShouldBe(1);
        pos.Top.ShouldBeFalse();
    }
}
