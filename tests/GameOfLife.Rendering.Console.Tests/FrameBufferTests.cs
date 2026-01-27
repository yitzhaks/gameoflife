using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class FrameBufferTests
{
    [Fact]
    public void Constructor_WithValidCapacity_CreatesBuffer()
    {
        var buffer = new FrameBuffer(10);

        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public void Constructor_WithZeroCapacity_Throws() => _ = Assert.Throws<ArgumentOutOfRangeException>(() => new FrameBuffer(0));

    [Fact]
    public void Constructor_WithNegativeCapacity_Throws() => _ = Assert.Throws<ArgumentOutOfRangeException>(() => new FrameBuffer(-1));

    [Fact]
    public void Add_WithinCapacity_AddsGlyph()
    {
        var buffer = new FrameBuffer(5);
        var glyph = new Glyph(null, 'A');

        buffer.Add(glyph);

        Assert.Equal(1, buffer.Count);
        Assert.Equal('A', buffer[0].Character);
    }

    [Fact]
    public void Add_ExceedsCapacity_ThrowsInvalidOperation()
    {
        var buffer = new FrameBuffer(2);
        buffer.Add(new Glyph(null, 'A'));
        buffer.Add(new Glyph(null, 'B'));

        // Adding a third glyph should throw
        _ = Assert.Throws<InvalidOperationException>(() => buffer.Add(new Glyph(null, 'C')));
    }

    [Fact]
    public void Add_ExceedsCapacity_DoesNotCorruptExistingData()
    {
        var buffer = new FrameBuffer(2);
        buffer.Add(new Glyph(null, 'A'));
        buffer.Add(new Glyph(null, 'B'));

        // Attempt to add beyond capacity
        try
        {
            buffer.Add(new Glyph(null, 'C'));
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Existing data should still be intact
        Assert.Equal(2, buffer.Count);
        Assert.Equal('A', buffer[0].Character);
        Assert.Equal('B', buffer[1].Character);
    }

    [Fact]
    public void Clear_ResetsCount()
    {
        var buffer = new FrameBuffer(5);
        buffer.Add(new Glyph(null, 'A'));
        buffer.Add(new Glyph(null, 'B'));

        buffer.Clear();

        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public void Clear_AllowsReuse()
    {
        var buffer = new FrameBuffer(2);
        buffer.Add(new Glyph(null, 'A'));
        buffer.Add(new Glyph(null, 'B'));

        buffer.Clear();

        // Should be able to add new glyphs
        buffer.Add(new Glyph(null, 'X'));
        buffer.Add(new Glyph(null, 'Y'));

        Assert.Equal(2, buffer.Count);
        Assert.Equal('X', buffer[0].Character);
        Assert.Equal('Y', buffer[1].Character);
    }

    [Fact]
    public void Indexer_ValidIndex_ReturnsGlyph()
    {
        var buffer = new FrameBuffer(3);
        buffer.Add(new Glyph(null, 'A'));
        buffer.Add(new Glyph(null, 'B'));
        buffer.Add(new Glyph(null, 'C'));

        Assert.Equal('A', buffer[0].Character);
        Assert.Equal('B', buffer[1].Character);
        Assert.Equal('C', buffer[2].Character);
    }

    [Fact]
    public void ForViewport_CalculatesCorrectCapacity()
    {
        // 10x5 viewport = 12x7 with borders = 84 chars + 7 newlines = 91
        var buffer = FrameBuffer.ForViewport(10, 5);

        // Should be able to add at least 91 glyphs without throwing
        for (int i = 0; i < 91; i++)
        {
            buffer.Add(new Glyph(null, '.'));
        }

        Assert.Equal(91, buffer.Count);
    }
}
