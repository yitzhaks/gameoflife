using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class IdentityLayoutEngineTests
{
    [Fact]
    public void CreateLayout_ValidTopology_ReturnsLayout()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new RectangularTopology((5, 5));

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        _ = layout.ShouldNotBeNull();
        _ = layout.Positions.ShouldNotBeNull();
        _ = layout.Bounds.ShouldNotBeNull();
    }

    [Fact]
    public void CreateLayout_NullTopology_ThrowsArgumentNullException()
    {
        var engine = new IdentityLayoutEngine();

        _ = Should.Throw<ArgumentNullException>(() => engine.CreateLayout(null!));
    }

    [Fact]
    public void CreateLayout_3x3Grid_BoundsAreCorrect()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new RectangularTopology((3, 3));

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        layout.Bounds.Min.ShouldBe(default);
        layout.Bounds.Max.ShouldBe((2, 2));
    }

    [Fact]
    public void CreateLayout_PositionsReturnIdentity()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new RectangularTopology((3, 3));

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        Point2D testPoint = (1, 2);
        layout.Positions[testPoint].ShouldBe(testPoint);
    }

    [Fact]
    public void CreateLayout_PositionsThrowForInvalidNode()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new RectangularTopology((3, 3));

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        _ = Should.Throw<KeyNotFoundException>(() => layout.Positions[(10, 10)]);
    }

    [Fact]
    public void CreateLayout_EnumerateNodesWithComparer_ReturnsOrderedNodes()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new RectangularTopology((3, 3));

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        // Sort by Y then X
        var comparer = Comparer<Point2D>.Create((a, b) =>
        {
            int yCompare = a.Y.CompareTo(b.Y);
            return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
        });

        var nodes = layout.EnumerateNodes(comparer).ToList();

        nodes.Count.ShouldBe(9);
        nodes[0].ShouldBe(default);
        nodes[1].ShouldBe((1, 0));
        nodes[2].ShouldBe((2, 0));
        nodes[3].ShouldBe((0, 1));
    }
}
