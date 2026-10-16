using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class HexLayoutEngineTests
{
    #region CreateLayout Tests

    [Fact]
    public void CreateLayout_NullTopology_ThrowsArgumentNullException()
    {
        var engine = new HexLayoutEngine();

        _ = Should.Throw<ArgumentNullException>(() => engine.CreateLayout(null!));
    }

    [Fact]
    public void CreateLayout_WithValidTopology_ReturnsLayout()
    {
        var engine = new HexLayoutEngine();
        var topology = new HexagonalTopology(3);

        ILayout<HexPoint, Point2D, HexBounds> layout = engine.CreateLayout(topology);

        _ = layout.ShouldNotBeNull();
    }

    [Fact]
    public void CreateLayout_LayoutHasCorrectBounds()
    {
        var engine = new HexLayoutEngine();
        var topology = new HexagonalTopology(2);

        ILayout<HexPoint, Point2D, HexBounds> layout = engine.CreateLayout(topology);

        // For radius 2, staggered mode:
        // Height: 2*2+1 = 5 rows
        // Width: (2*2+1)*2 = 10 chars
        layout.Bounds.Min.ShouldBe(default);
        layout.Bounds.Max.Y.ShouldBe(4); // 5 rows, 0-indexed
        layout.Bounds.Max.X.ShouldBe(9); // 10 chars, 0-indexed
    }

    #endregion

    #region Position Tests

    [Fact]
    public void Positions_OriginHasExpectedPosition()
    {
        var engine = new HexLayoutEngine();
        var topology = new HexagonalTopology(2);

        ILayout<HexPoint, Point2D, HexBounds> layout = engine.CreateLayout(topology);
        Point2D originPos = layout.Positions[default];

        // Origin (0, 0) at radius 2:
        // Y = r + radius = 0 + 2 = 2
        // X = (q + radius) * 2 + |r| = (0 + 2) * 2 + 0 = 4
        originPos.Y.ShouldBe(2);
        originPos.X.ShouldBe(4);
    }

    [Fact]
    public void Positions_AllNodesHavePositions()
    {
        var engine = new HexLayoutEngine();
        var topology = new HexagonalTopology(2);

        ILayout<HexPoint, Point2D, HexBounds> layout = engine.CreateLayout(topology);

        foreach (HexPoint node in topology.Nodes)
        {
            // Should not throw
            _ = layout.Positions[node];
        }
    }

    [Fact]
    public void Positions_EastNeighborHasCorrectPosition()
    {
        var engine = new HexLayoutEngine();
        var topology = new HexagonalTopology(2);

        ILayout<HexPoint, Point2D, HexBounds> layout = engine.CreateLayout(topology);
        Point2D originPos = layout.Positions[default];
        Point2D eastPos = layout.Positions[(1, 0)];

        // East neighbor (1, 0) should be 2 chars to the right (same row)
        eastPos.Y.ShouldBe(originPos.Y);
        eastPos.X.ShouldBe(originPos.X + 2);
    }

    [Fact]
    public void Positions_RowsAreStaggered()
    {
        var engine = new HexLayoutEngine();
        var topology = new HexagonalTopology(2);

        ILayout<HexPoint, Point2D, HexBounds> layout = engine.CreateLayout(topology);
        Point2D row0 = layout.Positions[default];       // r = 0
        Point2D rowMinus1 = layout.Positions[(0, -1)];  // r = -1

        // Row -1 should be indented by 1 relative to row 0
        // X offset for r=-1 is |r| = 1
        rowMinus1.Y.ShouldBe(row0.Y - 1); // One row up
    }

    [Fact]
    public void Positions_UnknownNode_ThrowsKeyNotFoundException()
    {
        var engine = new HexLayoutEngine();
        var topology = new HexagonalTopology(1);

        ILayout<HexPoint, Point2D, HexBounds> layout = engine.CreateLayout(topology);

        _ = Should.Throw<KeyNotFoundException>(() => _ = layout.Positions[(10, 10)]);
    }

    #endregion

    #region EnumerateNodes Tests

    [Fact]
    public void EnumerateNodes_ReturnsAllNodesFromTopology()
    {
        var engine = new HexLayoutEngine();
        var topology = new HexagonalTopology(2);

        ILayout<HexPoint, Point2D, HexBounds> layout = engine.CreateLayout(topology);
        var topologyNodes = topology.Nodes.ToHashSet();

        // Use a simple comparer that orders by R then Q
        IComparer<HexPoint> order = Comparer<HexPoint>.Create((a, b) =>
        {
            int rCompare = a.R.CompareTo(b.R);
            return rCompare != 0 ? rCompare : a.Q.CompareTo(b.Q);
        });

        var layoutNodes = layout.EnumerateNodes(order).ToList();

        layoutNodes.Count.ShouldBe(topology.CellCount);
        foreach (HexPoint node in layoutNodes)
        {
            topologyNodes.ShouldContain(node);
        }
    }

    #endregion

    #region ILayoutEngine Interface Tests

    [Fact]
    public void HexLayoutEngine_ImplementsILayoutEngine()
    {
        var engine = new HexLayoutEngine();

        _ = engine.ShouldBeAssignableTo<ILayoutEngine<HexagonalTopology, HexPoint, Point2D, HexBounds>>();
    }

    #endregion
}
