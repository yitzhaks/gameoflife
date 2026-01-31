using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

/// <summary>
/// Tests for <see cref="ViewportRenderer"/> class.
/// </summary>
public sealed class ViewportRendererTests
{
    #region Constructor with RectangularBounds

    [Fact]
    public void Constructor_RectangularBoundsWithNullViewport_UsesFullBounds()
    {
        var bounds = new RectangularBounds((0, 0), (9, 7));

        var renderer = new ViewportRenderer(bounds, null);

        renderer.RenderStartX.ShouldBe(0);
        renderer.RenderStartY.ShouldBe(0);
        renderer.RenderEndX.ShouldBe(9);
        renderer.RenderEndY.ShouldBe(7);
        renderer.Width.ShouldBe(10);
    }

    [Fact]
    public void Constructor_RectangularBoundsWithNullViewport_NonZeroMin_UsesFullBounds()
    {
        var bounds = new RectangularBounds((5, 3), (14, 10));

        var renderer = new ViewportRenderer(bounds, null);

        renderer.RenderStartX.ShouldBe(5);
        renderer.RenderStartY.ShouldBe(3);
        renderer.RenderEndX.ShouldBe(14);
        renderer.RenderEndY.ShouldBe(10);
        renderer.Width.ShouldBe(10);
    }

    [Fact]
    public void Constructor_RectangularBoundsWithViewport_ClipsToBounds()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 8, 20, 20);
        viewport.Move(5, 3);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.RenderStartX.ShouldBe(5);
        renderer.RenderStartY.ShouldBe(3);
        renderer.RenderEndX.ShouldBe(14);
        renderer.RenderEndY.ShouldBe(10);
        renderer.Width.ShouldBe(10);
    }

    [Fact]
    public void Constructor_RectangularBoundsWithViewport_ViewportExceedsBounds_ClipsToMax()
    {
        var bounds = new RectangularBounds((0, 0), (9, 9));
        var viewport = new Viewport(20, 20, 20, 20); // Viewport larger than bounds

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.RenderStartX.ShouldBe(0);
        renderer.RenderStartY.ShouldBe(0);
        renderer.RenderEndX.ShouldBe(9);
        renderer.RenderEndY.ShouldBe(9);
        renderer.Width.ShouldBe(10);
    }

    [Fact]
    public void Constructor_RectangularBoundsWithViewport_ViewportPartiallyOutOfBounds_ClipsToMax()
    {
        var bounds = new RectangularBounds((0, 0), (9, 9));
        var viewport = new Viewport(10, 10, 15, 15);
        viewport.Move(5, 5); // Moves to offset (5,5), so viewport would extend to (14,14) but board ends at (9,9)

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.RenderEndX.ShouldBe(9);
        renderer.RenderEndY.ShouldBe(9);
    }

    #endregion

    #region Constructor with PackedBounds

    [Fact]
    public void Constructor_PackedBoundsWithNullViewport_UsesFullBounds()
    {
        var bounds = new PackedBounds(10, 8);

        var renderer = new ViewportRenderer(bounds, null);

        renderer.RenderStartX.ShouldBe(0);
        renderer.RenderStartY.ShouldBe(0);
        renderer.RenderEndX.ShouldBe(9);
        renderer.RenderEndY.ShouldBe(3); // Height is 8/2 = 4, so max Y is 3
        renderer.Width.ShouldBe(10);
    }

    [Fact]
    public void Constructor_PackedBoundsWithViewport_ClipsToBounds()
    {
        var bounds = new PackedBounds(20, 20);
        var viewport = new Viewport(10, 8, 20, 10);
        viewport.Move(5, 2);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.RenderStartX.ShouldBe(5);
        renderer.RenderStartY.ShouldBe(2);
        renderer.RenderEndX.ShouldBe(14);
        renderer.RenderEndY.ShouldBe(9);
        renderer.Width.ShouldBe(10);
    }

    [Fact]
    public void Constructor_PackedBoundsWithViewport_ViewportExceedsBounds_ClipsToMax()
    {
        var bounds = new PackedBounds(10, 8);
        var viewport = new Viewport(20, 20, 30, 30);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.RenderStartX.ShouldBe(0);
        renderer.RenderStartY.ShouldBe(0);
        renderer.RenderEndX.ShouldBe(9);
        renderer.RenderEndY.ShouldBe(3);
        renderer.Width.ShouldBe(10);
    }

    [Fact]
    public void Constructor_PackedBoundsWithViewport_ViewportPartiallyOutOfBounds_ClipsToMax()
    {
        var bounds = new PackedBounds(10, 8);
        var viewport = new Viewport(8, 6, 15, 15);
        viewport.Move(5, 5);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.RenderEndX.ShouldBe(9);
        renderer.RenderEndY.ShouldBe(3);
    }

    #endregion

    #region Edge detection with null viewport

    [Fact]
    public void IsAtTop_NullViewport_ReturnsTrue()
    {
        var bounds = new RectangularBounds((0, 0), (9, 9));

        var renderer = new ViewportRenderer(bounds, null);

        renderer.IsAtTop.ShouldBeTrue();
    }

    [Fact]
    public void IsAtBottom_NullViewport_ReturnsTrue()
    {
        var bounds = new RectangularBounds((0, 0), (9, 9));

        var renderer = new ViewportRenderer(bounds, null);

        renderer.IsAtBottom.ShouldBeTrue();
    }

    [Fact]
    public void IsAtLeft_NullViewport_ReturnsTrue()
    {
        var bounds = new RectangularBounds((0, 0), (9, 9));

        var renderer = new ViewportRenderer(bounds, null);

        renderer.IsAtLeft.ShouldBeTrue();
    }

    [Fact]
    public void IsAtRight_NullViewport_ReturnsTrue()
    {
        var bounds = new RectangularBounds((0, 0), (9, 9));

        var renderer = new ViewportRenderer(bounds, null);

        renderer.IsAtRight.ShouldBeTrue();
    }

    #endregion

    #region Edge detection with viewport at top-left corner

    [Fact]
    public void IsAtTop_ViewportAtTopLeftCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 10, 20, 20);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtTop.ShouldBeTrue();
    }

    [Fact]
    public void IsAtLeft_ViewportAtTopLeftCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 10, 20, 20);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtLeft.ShouldBeTrue();
    }

    [Fact]
    public void IsAtBottom_ViewportAtTopLeftCorner_ReturnsFalse()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 10, 20, 20);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtBottom.ShouldBeFalse();
    }

    [Fact]
    public void IsAtRight_ViewportAtTopLeftCorner_ReturnsFalse()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 10, 20, 20);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtRight.ShouldBeFalse();
    }

    #endregion

    #region Edge detection with viewport at bottom-right corner

    [Fact]
    public void IsAtTop_ViewportAtBottomRightCorner_ReturnsFalse()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 10, 20, 20);
        viewport.Move(10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtTop.ShouldBeFalse();
    }

    [Fact]
    public void IsAtLeft_ViewportAtBottomRightCorner_ReturnsFalse()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 10, 20, 20);
        viewport.Move(10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtLeft.ShouldBeFalse();
    }

    [Fact]
    public void IsAtBottom_ViewportAtBottomRightCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 10, 20, 20);
        viewport.Move(10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtBottom.ShouldBeTrue();
    }

    [Fact]
    public void IsAtRight_ViewportAtBottomRightCorner_ReturnsTrue()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(10, 10, 20, 20);
        viewport.Move(10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtRight.ShouldBeTrue();
    }

    #endregion

    #region Edge detection with viewport in center

    [Fact]
    public void IsAtTop_ViewportInCenter_ReturnsFalse()
    {
        var bounds = new RectangularBounds((0, 0), (29, 29));
        var viewport = new Viewport(10, 10, 30, 30);
        viewport.Move(10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtTop.ShouldBeFalse();
    }

    [Fact]
    public void IsAtBottom_ViewportInCenter_ReturnsFalse()
    {
        var bounds = new RectangularBounds((0, 0), (29, 29));
        var viewport = new Viewport(10, 10, 30, 30);
        viewport.Move(10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtBottom.ShouldBeFalse();
    }

    [Fact]
    public void IsAtLeft_ViewportInCenter_ReturnsFalse()
    {
        var bounds = new RectangularBounds((0, 0), (29, 29));
        var viewport = new Viewport(10, 10, 30, 30);
        viewport.Move(10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtLeft.ShouldBeFalse();
    }

    [Fact]
    public void IsAtRight_ViewportInCenter_ReturnsFalse()
    {
        var bounds = new RectangularBounds((0, 0), (29, 29));
        var viewport = new Viewport(10, 10, 30, 30);
        viewport.Move(10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtRight.ShouldBeFalse();
    }

    #endregion

    #region Edge detection with viewport covering entire board

    [Fact]
    public void EdgeDetection_ViewportCoversEntireBoard_AllEdgesTrue()
    {
        var bounds = new RectangularBounds((0, 0), (9, 9));
        var viewport = new Viewport(10, 10, 10, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.IsAtTop.ShouldBeTrue();
        renderer.IsAtBottom.ShouldBeTrue();
        renderer.IsAtLeft.ShouldBeTrue();
        renderer.IsAtRight.ShouldBeTrue();
    }

    #endregion

    #region Width calculation

    [Fact]
    public void Width_RectangularBoundsNullViewport_ReturnsCorrectWidth()
    {
        var bounds = new RectangularBounds((0, 0), (14, 9));

        var renderer = new ViewportRenderer(bounds, null);

        renderer.Width.ShouldBe(15);
    }

    [Fact]
    public void Width_RectangularBoundsWithViewport_ReturnsViewportWidth()
    {
        var bounds = new RectangularBounds((0, 0), (19, 19));
        var viewport = new Viewport(8, 6, 20, 20);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.Width.ShouldBe(8);
    }

    [Fact]
    public void Width_PackedBoundsNullViewport_ReturnsCorrectWidth()
    {
        var bounds = new PackedBounds(12, 10);

        var renderer = new ViewportRenderer(bounds, null);

        renderer.Width.ShouldBe(12);
    }

    [Fact]
    public void Width_PackedBoundsWithViewport_ReturnsViewportWidth()
    {
        var bounds = new PackedBounds(20, 20);
        var viewport = new Viewport(8, 6, 20, 10);

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.Width.ShouldBe(8);
    }

    [Fact]
    public void Width_ViewportClippedByBounds_ReturnsClippedWidth()
    {
        var bounds = new RectangularBounds((0, 0), (4, 4));
        var viewport = new Viewport(10, 10, 20, 20);
        viewport.Move(2, 0); // Viewport starts at x=2, bounds end at x=4, so width should be 3

        var renderer = new ViewportRenderer(bounds, viewport);

        renderer.Width.ShouldBe(3);
    }

    #endregion

    #region Edge detection with PackedBounds

    [Fact]
    public void IsAtTop_PackedBoundsNullViewport_ReturnsTrue()
    {
        var bounds = new PackedBounds(10, 10);

        var renderer = new ViewportRenderer(bounds, null);

        renderer.IsAtTop.ShouldBeTrue();
    }

    [Fact]
    public void IsAtBottom_PackedBoundsNullViewport_ReturnsTrue()
    {
        var bounds = new PackedBounds(10, 10);

        var renderer = new ViewportRenderer(bounds, null);

        renderer.IsAtBottom.ShouldBeTrue();
    }

    [Fact]
    public void IsAtLeft_PackedBoundsNullViewport_ReturnsTrue()
    {
        var bounds = new PackedBounds(10, 10);

        var renderer = new ViewportRenderer(bounds, null);

        renderer.IsAtLeft.ShouldBeTrue();
    }

    [Fact]
    public void IsAtRight_PackedBoundsNullViewport_ReturnsTrue()
    {
        var bounds = new PackedBounds(10, 10);

        var renderer = new ViewportRenderer(bounds, null);

        renderer.IsAtRight.ShouldBeTrue();
    }

    #endregion
}
