using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ViewportTests
{
    [Fact]
    public void Constructor_ValidDimensions_CreatesViewport()
    {
        var viewport = new Viewport(10, 8, 100, 50);

        Assert.Equal(0, viewport.OffsetX);
        Assert.Equal(0, viewport.OffsetY);
        Assert.Equal(10, viewport.Width);
        Assert.Equal(8, viewport.Height);
        Assert.Equal(100, viewport.BoardWidth);
        Assert.Equal(50, viewport.BoardHeight);
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
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Viewport(viewportWidth, viewportHeight, boardWidth, boardHeight));
    }

    [Fact]
    public void IsAtTop_AtTopEdge_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        Assert.True(viewport.IsAtTop);
    }

    [Fact]
    public void IsAtTop_NotAtTopEdge_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(0, 1);
        Assert.False(viewport.IsAtTop);
    }

    [Fact]
    public void IsAtBottom_AtBottomEdge_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(0, 90); // Move to bottom edge
        Assert.True(viewport.IsAtBottom);
    }

    [Fact]
    public void IsAtBottom_NotAtBottomEdge_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        Assert.False(viewport.IsAtBottom);
    }

    [Fact]
    public void IsAtLeft_AtLeftEdge_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        Assert.True(viewport.IsAtLeft);
    }

    [Fact]
    public void IsAtLeft_NotAtLeftEdge_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(1, 0);
        Assert.False(viewport.IsAtLeft);
    }

    [Fact]
    public void IsAtRight_AtRightEdge_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(90, 0); // Move to right edge
        Assert.True(viewport.IsAtRight);
    }

    [Fact]
    public void IsAtRight_NotAtRightEdge_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        Assert.False(viewport.IsAtRight);
    }

    [Fact]
    public void Move_PositiveDeltas_MovesViewport()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(5, 3);

        Assert.Equal(5, viewport.OffsetX);
        Assert.Equal(3, viewport.OffsetY);
    }

    [Fact]
    public void Move_NegativeDeltas_MovesViewport()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(50, 50);
        viewport.Move(-10, -5);

        Assert.Equal(40, viewport.OffsetX);
        Assert.Equal(45, viewport.OffsetY);
    }

    [Fact]
    public void Move_PastLeftEdge_ClampsToZero()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(-100, 0);

        Assert.Equal(0, viewport.OffsetX);
    }

    [Fact]
    public void Move_PastTopEdge_ClampsToZero()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(0, -100);

        Assert.Equal(0, viewport.OffsetY);
    }

    [Fact]
    public void Move_PastRightEdge_ClampsToMaxOffset()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(1000, 0);

        Assert.Equal(90, viewport.OffsetX); // 100 - 10 = 90
    }

    [Fact]
    public void Move_PastBottomEdge_ClampsToMaxOffset()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(0, 1000);

        Assert.Equal(90, viewport.OffsetY); // 100 - 10 = 90
    }

    [Fact]
    public void Move_ViewportLargerThanBoard_ClampsToZero()
    {
        var viewport = new Viewport(100, 100, 50, 50);
        viewport.Move(10, 10);

        Assert.Equal(0, viewport.OffsetX);
        Assert.Equal(0, viewport.OffsetY);
    }

    [Fact]
    public void Contains_PointInViewport_ReturnsTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(20, 30);

        Assert.True(viewport.Contains(25, 35)); // Inside viewport
    }

    [Fact]
    public void Contains_PointOutsideViewport_ReturnsFalse()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(20, 30);

        Assert.False(viewport.Contains(15, 35)); // Left of viewport
        Assert.False(viewport.Contains(35, 35)); // Right of viewport
        Assert.False(viewport.Contains(25, 25)); // Above viewport
        Assert.False(viewport.Contains(25, 45)); // Below viewport
    }

    [Fact]
    public void Contains_PointAtViewportEdges_ReturnsCorrectly()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(20, 30);

        // Left edge (inclusive)
        Assert.True(viewport.Contains(20, 35));
        // Right edge (exclusive: 20 + 10 = 30)
        Assert.False(viewport.Contains(30, 35));
        // Top edge (inclusive)
        Assert.True(viewport.Contains(25, 30));
        // Bottom edge (exclusive: 30 + 10 = 40)
        Assert.False(viewport.Contains(25, 40));
    }

    [Fact]
    public void EdgeDetection_ViewportSameAsBoard_AllEdgesTrue()
    {
        var viewport = new Viewport(50, 50, 50, 50);

        Assert.True(viewport.IsAtTop);
        Assert.True(viewport.IsAtBottom);
        Assert.True(viewport.IsAtLeft);
        Assert.True(viewport.IsAtRight);
    }

    [Fact]
    public void EdgeDetection_ViewportInCenter_NoEdgesTrue()
    {
        var viewport = new Viewport(10, 10, 100, 100);
        viewport.Move(45, 45);

        Assert.False(viewport.IsAtTop);
        Assert.False(viewport.IsAtBottom);
        Assert.False(viewport.IsAtLeft);
        Assert.False(viewport.IsAtRight);
    }
}
