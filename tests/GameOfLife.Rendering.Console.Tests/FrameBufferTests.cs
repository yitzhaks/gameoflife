using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class FrameBufferTests
{
    [Fact]
    public void Constructor_WithValidCapacity_CreatesBuffer()
    {
        var buffer = new FrameBuffer(10);

        buffer.Count.ShouldBe(0);
    }

    [Fact]
    public void Constructor_WithZeroCapacity_Throws() => _ = Should.Throw<ArgumentOutOfRangeException>(() => new FrameBuffer(0));

    [Fact]
    public void Constructor_WithNegativeCapacity_Throws() => _ = Should.Throw<ArgumentOutOfRangeException>(() => new FrameBuffer(-1));

    [Theory]
    [AutoNSubstituteData]
    public void Add_WithinCapacity_AddsGlyph(Glyph glyph)
    {
        var buffer = new FrameBuffer(5);

        buffer.Add(glyph);

        buffer.Count.ShouldBe(1);
        buffer[0].ShouldBe(glyph);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Add_ExceedsCapacity_ThrowsInvalidOperation(Glyph glyph1, Glyph glyph2, Glyph glyph3)
    {
        var buffer = new FrameBuffer(2);
        buffer.Add(glyph1);
        buffer.Add(glyph2);

        // Adding a third glyph should throw
        _ = Should.Throw<InvalidOperationException>(() => buffer.Add(glyph3));
    }

    [Theory]
    [AutoNSubstituteData]
    public void Add_ExceedsCapacity_DoesNotCorruptExistingData(Glyph glyph1, Glyph glyph2, Glyph glyph3)
    {
        var buffer = new FrameBuffer(2);
        buffer.Add(glyph1);
        buffer.Add(glyph2);

        // Attempt to add beyond capacity
        try
        {
            buffer.Add(glyph3);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Existing data should still be intact
        buffer.Count.ShouldBe(2);
        buffer[0].ShouldBe(glyph1);
        buffer[1].ShouldBe(glyph2);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Clear_ResetsCount(Glyph glyph1, Glyph glyph2)
    {
        var buffer = new FrameBuffer(5);
        buffer.Add(glyph1);
        buffer.Add(glyph2);

        buffer.Clear();

        buffer.Count.ShouldBe(0);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Clear_AllowsReuse(Glyph glyph1, Glyph glyph2, Glyph glyph3, Glyph glyph4)
    {
        var buffer = new FrameBuffer(2);
        buffer.Add(glyph1);
        buffer.Add(glyph2);

        buffer.Clear();

        // Should be able to add new glyphs
        buffer.Add(glyph3);
        buffer.Add(glyph4);

        buffer.Count.ShouldBe(2);
        buffer[0].ShouldBe(glyph3);
        buffer[1].ShouldBe(glyph4);
    }

    [Theory]
    [AutoNSubstituteData]
    public void Indexer_ValidIndex_ReturnsGlyph(Glyph glyph1, Glyph glyph2, Glyph glyph3)
    {
        var buffer = new FrameBuffer(3);
        buffer.Add(glyph1);
        buffer.Add(glyph2);
        buffer.Add(glyph3);

        buffer[0].ShouldBe(glyph1);
        buffer[1].ShouldBe(glyph2);
        buffer[2].ShouldBe(glyph3);
    }

    [Theory]
    [AutoNSubstituteData]
    public void ForViewport_CalculatesCorrectCapacity(Glyph glyph)
    {
        // 10x5 viewport = 12x7 with borders = 84 chars + 7 newlines = 91
        var buffer = FrameBuffer.ForViewport(10, 5);

        // Should be able to add at least 91 glyphs without throwing
        for (int i = 0; i < 91; i++)
        {
            buffer.Add(glyph);
        }

        buffer.Count.ShouldBe(91);
    }
}
