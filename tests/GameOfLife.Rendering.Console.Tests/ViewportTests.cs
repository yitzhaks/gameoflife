using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ViewportTests
{
    [Fact]
    public void Constructor_ValidDimensions_CreatesViewport()
    {
        var viewport = new Viewport(10, 8, 100, 50);

        viewport.OffsetX.ShouldBe(0);
        viewport.OffsetY.ShouldBe(0);
        viewport.Width.ShouldBe(10);
        viewport.Height.ShouldBe(8);
        viewport.BoardWidth.ShouldBe(100);
        viewport.BoardHeight.ShouldBe(50);
    }

    [Theory]
    [InlineData(0, 5, 100, 50)]
    [InlineData(-1, 5, 100, 50)]
    [InlineData(5, 0, 100, 50)]
    [InlineData(5, -1, 100, 50)]
    [InlineData(5, 5, 0, 50)]
    [InlineData(5, 5, -1, 50)]
    [InlineData(5, 5, 100, 0)]
    [InlineData(5, 5, 100, -1)]
    public void Constructor_InvalidDimensions_ThrowsArgumentOutOfRangeException(
        int viewportWidth, int viewportHeight, int boardWidth, int boardHeight)
    {
        _ = Should.Throw<ArgumentOutOfRangeException>(() =>
            new Viewport(viewportWidth, viewportHeight, boardWidth, boardHeight));
    }

    [Fact]
    public void IsAtTop_AtTopEdge_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.IsAtTop.ShouldBeTrue();
    }

    [Fact]
    public void IsAtTop_NotAtTopEdge_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(0, 1);
        viewport.IsAtTop.ShouldBeFalse();
    }

    [Fact]
    public void IsAtBottom_AtBottomEdge_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(0, 90); // Move to bottom edge
        viewport.IsAtBottom.ShouldBeTrue();
    }

    [Fact]
    public void IsAtBottom_NotAtBottomEdge_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.IsAtBottom.ShouldBeFalse();
    }

    [Fact]
    public void IsAtLeft_AtLeftEdge_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.IsAtLeft.ShouldBeTrue();
    }

    [Fact]
    public void IsAtLeft_NotAtLeftEdge_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(1, 0);
        viewport.IsAtLeft.ShouldBeFalse();
    }

    [Fact]
    public void IsAtRight_AtRightEdge_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(90, 0); // Move to right edge
        viewport.IsAtRight.ShouldBeTrue();
    }

    [Fact]
    public void IsAtRight_NotAtRightEdge_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.IsAtRight.ShouldBeFalse();
    }

    [Fact]
    public void Move_PositiveDeltas_MovesViewport()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(5, 3);

        viewport.OffsetX.ShouldBe(5);
        viewport.OffsetY.ShouldBe(3);
    }

    [Fact]
    public void Move_NegativeDeltas_MovesViewport()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(50, 50);
        viewport.Move(-10, -5);

        viewport.OffsetX.ShouldBe(40);
        viewport.OffsetY.ShouldBe(45);
    }

    [Fact]
    public void Move_PastLeftEdge_ClampsToZero()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(-100, 0);

        viewport.OffsetX.ShouldBe(0);
    }

    [Fact]
    public void Move_PastTopEdge_ClampsToZero()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(0, -100);

        viewport.OffsetY.ShouldBe(0);
    }

    [Fact]
    public void Move_PastRightEdge_ClampsToMaxOffset()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(1000, 0);

        viewport.OffsetX.ShouldBe(90); // 100 - 10 = 90
    }

    [Fact]
    public void Move_PastBottomEdge_ClampsToMaxOffset()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(0, 1000);

        viewport.OffsetY.ShouldBe(90); // 100 - 10 = 90
    }

    [Fact]
    public void Move_ViewportLargerThanBoard_ClampsToZero()
    {
        var viewport = new Viewport(100, 100, 50, 50);
        viewport.Move(10, 10);

        viewport.OffsetX.ShouldBe(0);
        viewport.OffsetY.ShouldBe(0);
    }

    [Fact]
    public void Contains_PointInViewport_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(20, 30);

        viewport.Contains(25, 35).ShouldBeTrue(); // Inside viewport
    }

    [Fact]
    public void Contains_PointOutsideViewport_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(20, 30);

        viewport.Contains(15, 35).ShouldBeFalse(); // Left of viewport
        viewport.Contains(35, 35).ShouldBeFalse(); // Right of viewport
        viewport.Contains(25, 25).ShouldBeFalse(); // Above viewport
        viewport.Contains(25, 45).ShouldBeFalse(); // Below viewport
    }

    [Fact]
    public void Contains_PointAtViewportEdges_ReturnsCorrectly()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(20, 30);

        // Left edge (inclusive)
        viewport.Contains(20, 35).ShouldBeTrue();
        // Right edge (exclusive: 20 + 10 = 30)
        viewport.Contains(30, 35).ShouldBeFalse();
        // Top edge (inclusive)
        viewport.Contains(25, 30).ShouldBeTrue();
        // Bottom edge (exclusive: 30 + 10 = 40)
        viewport.Contains(25, 40).ShouldBeFalse();
    }

    [Fact]
    public void EdgeDetection_ViewportSameAsBoard_AllEdgesTrue()
    {
        var viewport = new Viewport(50, 50, 50, 50);

        viewport.IsAtTop.ShouldBeTrue();
        viewport.IsAtBottom.ShouldBeTrue();
        viewport.IsAtLeft.ShouldBeTrue();
        viewport.IsAtRight.ShouldBeTrue();
    }

    [Fact]
    public void EdgeDetection_ViewportInCenter_NoEdgesTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(45, 45);

        viewport.IsAtTop.ShouldBeFalse();
        viewport.IsAtBottom.ShouldBeFalse();
        viewport.IsAtLeft.ShouldBeFalse();
        viewport.IsAtRight.ShouldBeFalse();
    }
}
