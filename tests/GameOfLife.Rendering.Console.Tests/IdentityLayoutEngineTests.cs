using GameOfLife.Core;
using GameOfLife.Rendering;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class IdentityLayoutEngineTests
{
    [Fact]
    public void CreateLayout_ValidTopology_ReturnsLayout()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new Grid2DTopology(5, 5);

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        Assert.NotNull(layout);
        Assert.NotNull(layout.Positions);
        Assert.NotNull(layout.Bounds);
    }

    [Fact]
    public void CreateLayout_NullTopology_ThrowsArgumentNullException()
    {
        var engine = new IdentityLayoutEngine();

        _ = Assert.Throws<ArgumentNullException>(() => engine.CreateLayout(null!));
    }

    [Fact]
    public void CreateLayout_3x3Grid_BoundsAreCorrect()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new Grid2DTopology(3, 3);

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        Assert.Equal(new Point2D(0, 0), layout.Bounds.Min);
        Assert.Equal(new Point2D(2, 2), layout.Bounds.Max);
    }

    [Fact]
    public void CreateLayout_PositionsReturnIdentity()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new Grid2DTopology(3, 3);

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        var testPoint = new Point2D(1, 2);
        Assert.Equal(testPoint, layout.Positions[testPoint]);
    }

    [Fact]
    public void CreateLayout_PositionsThrowForInvalidNode()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new Grid2DTopology(3, 3);

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        _ = Assert.Throws<KeyNotFoundException>(() => layout.Positions[new Point2D(10, 10)]);
    }

    [Fact]
    public void CreateLayout_EnumerateNodesWithComparer_ReturnsOrderedNodes()
    {
        var engine = new IdentityLayoutEngine();
        var topology = new Grid2DTopology(3, 3);

        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);

        // Sort by Y then X
        var comparer = Comparer<Point2D>.Create((a, b) =>
        {
            int yCompare = a.Y.CompareTo(b.Y);
            return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
        });

        var nodes = layout.EnumerateNodes(comparer).ToList();

        Assert.Equal(9, nodes.Count);
        Assert.Equal(new Point2D(0, 0), nodes[0]);
        Assert.Equal(new Point2D(1, 0), nodes[1]);
        Assert.Equal(new Point2D(2, 0), nodes[2]);
        Assert.Equal(new Point2D(0, 1), nodes[3]);
    }
}
