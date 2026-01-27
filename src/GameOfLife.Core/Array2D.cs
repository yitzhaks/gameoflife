namespace GameOfLife.Core;

/// <summary>
/// A lightweight read-only wrapper providing 2D indexing over a 1D array.
/// Does not own the array - caller is responsible for lifecycle.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal readonly ref struct ReadOnlyArray2D<T> where T : struct
{
    private readonly ReadOnlySpan<T> _data;

    /// <summary>
    /// Creates a new read-only 2D view over a 1D array.
    /// </summary>
    /// <param name="data">The underlying 1D array.</param>
    /// <param name="size">The size (width and height) of the 2D view.</param>
    /// <exception cref="ArgumentException">Thrown when the array is too small for the specified dimensions.</exception>
    public ReadOnlyArray2D(T[] data, Size2D size)
    {
        // ArrayPool.Rent() returns arrays at least as large as requested, so we allow larger arrays
        // and slice to the exact size needed for the 2D view.
        int requiredLength = size.Width * size.Height;
        if (data.Length < requiredLength)
        {
            throw new ArgumentException($"Array length {data.Length} is smaller than required {requiredLength} for {size.Width}x{size.Height} grid.", nameof(data));
        }

        _data = data.AsSpan(0, requiredLength);
        Size = size;
    }

    /// <summary>
    /// Gets the size of the 2D view.
    /// </summary>
    public Size2D Size { get; }

    /// <summary>
    /// Gets the element at the specified point.
    /// </summary>
    /// <param name="point">The point coordinates.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are out of bounds.</exception>
    public T this[Point2D point]
    {
        get
        {
            if (!point.IsInBounds(Size))
            {
                throw new ArgumentOutOfRangeException(nameof(point), $"Coordinates ({point.X}, {point.Y}) are out of bounds for grid of size {Size.Width}x{Size.Height}.");
            }

            return _data[(point.Y * Size.Width) + point.X];
        }
    }

    /// <summary>
    /// Gets the element at the specified point, or the default value if out of bounds.
    /// </summary>
    /// <param name="point">The point coordinates.</param>
    /// <param name="defaultValue">The value to return if coordinates are out of bounds.</param>
    public T GetOrDefault(Point2D point, T defaultValue = default) => point.IsInBounds(Size) ? _data[(point.Y * Size.Width) + point.X] : defaultValue;
}

/// <summary>
/// A lightweight mutable wrapper providing 2D indexing over a 1D array.
/// Does not own the array - caller is responsible for lifecycle.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal readonly ref struct Array2D<T> where T : struct
{
    private readonly Span<T> _data;

    /// <summary>
    /// Creates a new 2D view over a 1D array.
    /// </summary>
    /// <param name="data">The underlying 1D array.</param>
    /// <param name="size">The size (width and height) of the 2D view.</param>
    /// <exception cref="ArgumentException">Thrown when the array is too small for the specified dimensions.</exception>
    public Array2D(T[] data, Size2D size)
    {
        // ArrayPool.Rent() returns arrays at least as large as requested, so we allow larger arrays
        // and slice to the exact size needed for the 2D view.
        int requiredLength = size.Width * size.Height;
        if (data.Length < requiredLength)
        {
            throw new ArgumentException($"Array length {data.Length} is smaller than required {requiredLength} for {size.Width}x{size.Height} grid.", nameof(data));
        }

        _data = data.AsSpan(0, requiredLength);
        Size = size;
    }

    /// <summary>
    /// Gets the size of the 2D view.
    /// </summary>
    public Size2D Size { get; }

    /// <summary>
    /// Gets or sets the element at the specified point.
    /// </summary>
    /// <param name="point">The point coordinates.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are out of bounds.</exception>
    public ref T this[Point2D point]
    {
        get
        {
            if (!point.IsInBounds(Size))
            {
                throw new ArgumentOutOfRangeException(nameof(point), $"Coordinates ({point.X}, {point.Y}) are out of bounds for grid of size {Size.Width}x{Size.Height}.");
            }

            return ref _data[(point.Y * Size.Width) + point.X];
        }
    }
}
