using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class HalfBlockLayoutTests
{
    [Fact]
    public void Constructor_EvenHeight_Succeeds()
    {
        var topology = new RectangularTopology((4, 6));

        var layout = new HalfBlockLayout(topology);

        _ = layout.ShouldNotBeNull();
        layout.Bounds.Width.ShouldBe(4);
        layout.Bounds.Height.ShouldBe(3);
    }

    [Fact]
    public void Constructor_MinimumValidHeight2_Succeeds()
    {
        var topology = new RectangularTopology((4, 2));

        var layout = new HalfBlockLayout(topology);

        layout.Bounds.Height.ShouldBe(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void Constructor_OddHeight_ThrowsArgumentException(int height)
    {
        var topology = new RectangularTopology((4, height));

        ArgumentException exception = Should.Throw<ArgumentException>(() => new HalfBlockLayout(topology));

        exception.Message.ShouldContain("even");
    }

    [Fact]
    public void Constructor_NullTopology_ThrowsArgumentNullException() => _ = Should.Throw<ArgumentNullException>(() => new HalfBlockLayout(null!));

    [Fact]
    public void Positions_MapsTopRowsCorrectly()
    {
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);

        // Y=0 should map to packed Y=0, Top=true
        PackedPoint2D pos = layout.Positions[(0, 0)];
        pos.Y.ShouldBe(0);
        pos.Top.ShouldBeTrue();

        // Y=2 should map to packed Y=1, Top=true
        pos = layout.Positions[(0, 2)];
        pos.Y.ShouldBe(1);
        pos.Top.ShouldBeTrue();
    }

    [Fact]
    public void Positions_MapsBottomRowsCorrectly()
    {
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);

        // Y=1 should map to packed Y=0, Top=false
        PackedPoint2D pos = layout.Positions[(0, 1)];
        pos.Y.ShouldBe(0);
        pos.Top.ShouldBeFalse();

        // Y=3 should map to packed Y=1, Top=false
        pos = layout.Positions[(0, 3)];
        pos.Y.ShouldBe(1);
        pos.Top.ShouldBeFalse();
    }

    [Fact]
    public void Positions_ThrowsForInvalidNode()
    {
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);

        _ = Should.Throw<KeyNotFoundException>(() => layout.Positions[(10, 10)]);
    }

    [Fact]
    public void EnumerateNodes_ReturnsAllNodes()
    {
        var topology = new RectangularTopology((3, 4));
        var layout = new HalfBlockLayout(topology);

        var comparer = Comparer<Point2D>.Create((a, b) =>
        {
            int yCompare = a.Y.CompareTo(b.Y);
            return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
        });

        var nodes = layout.EnumerateNodes(comparer).ToList();

        nodes.Count.ShouldBe(12); // 3x4 = 12 nodes
        nodes[0].ShouldBe((0, 0));
        nodes[1].ShouldBe((1, 0));
        nodes[2].ShouldBe((2, 0));
        nodes[3].ShouldBe((0, 1));
    }

    [Fact]
    public void EnumerateNodes_NullComparer_ThrowsArgumentNullException()
    {
        var topology = new RectangularTopology((3, 4));
        var layout = new HalfBlockLayout(topology);

        _ = Should.Throw<ArgumentNullException>(() => layout.EnumerateNodes(null!).ToList());
    }

    [Fact]
    public void Bounds_4x4Grid_HasCorrectPackedDimensions()
    {
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);

        layout.Bounds.Width.ShouldBe(4);
        layout.Bounds.Height.ShouldBe(2);
        layout.Bounds.OriginalHeight.ShouldBe(4);
    }
}
