namespace GameOfLife.Rendering.Console;

/// <summary>
/// A pre-allocated buffer for storing frame glyphs.
/// Avoids per-frame allocations by reusing a fixed-size array.
/// </summary>
public sealed class FrameBuffer
{
    private readonly Glyph[] _glyphs;

    /// <summary>
    /// Creates a new frame buffer with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of glyphs this buffer can hold.</param>
    public FrameBuffer(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _glyphs = new Glyph[capacity];
    }

    /// <summary>
    /// Creates a frame buffer sized for a viewport with borders.
    /// </summary>
    /// <param name="viewportWidth">The viewport width (content area).</param>
    /// <param name="viewportHeight">The viewport height (content area).</param>
    /// <returns>A frame buffer with appropriate capacity.</returns>
    public static FrameBuffer ForViewport(int viewportWidth, int viewportHeight)
    {
        // Width: content + 2 borders
        // Height: content + 2 borders
        // Plus newlines at end of each row
        int width = viewportWidth + 2;
        int height = viewportHeight + 2;
        int capacity = (width * height) + height; // chars + newlines
        return new FrameBuffer(capacity);
    }

    /// <summary>
    /// Gets the number of glyphs currently in the buffer.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the glyph at the specified index.
    /// </summary>
    public Glyph this[int index] => _glyphs[index];

    /// <summary>
    /// Clears the buffer for reuse.
    /// </summary>
    public void Clear() => Count = 0;

    /// <summary>
    /// Adds a glyph to the buffer.
    /// </summary>
    /// <param name="glyph">The glyph to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the buffer is full.</exception>
    public void Add(Glyph glyph)
    {
        if (Count >= _glyphs.Length)
        {
            throw new InvalidOperationException($"Frame buffer is full (capacity: {_glyphs.Length}).");
        }

        _glyphs[Count++] = glyph;
    }
}
