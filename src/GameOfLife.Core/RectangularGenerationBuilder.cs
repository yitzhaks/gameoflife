using System.Buffers;

namespace GameOfLife.Core;

/// <summary>
/// A mutable builder for creating RectangularGeneration instances.
/// Rents arrays from ArrayPool for efficient memory usage.
/// Must be disposed if Build() is not called.
/// </summary>
public sealed class RectangularGenerationBuilder : IDisposable
{
    private bool[] _states;
    private bool _built;

    /// <summary>
    /// Creates a new builder for the specified grid dimensions.
    /// Rents an array from the shared ArrayPool.
    /// </summary>
    /// <param name="size">The size of the grid.</param>
    /// <param name="clear">Whether to clear the array. Set to false if all cells will be explicitly set.</param>
    public RectangularGenerationBuilder(Size2D size, bool clear = true)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size.Width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(size.Height, 1);

        Size = size;
        int arraySize = size.Width * size.Height;
        _states = ArrayPool<bool>.Shared.Rent(arraySize);
        if (clear)
        {
            Array.Clear(_states, 0, arraySize);
        }
    }

    /// <summary>
    /// Gets the size of the grid.
    /// </summary>
    public Size2D Size { get; }

    /// <summary>
    /// Gets or sets the state at the specified point.
    /// </summary>
    /// <param name="point">The point coordinates.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are out of bounds.</exception>
    /// <exception cref="InvalidOperationException">Thrown when setting after Build() has been called.</exception>
    public bool this[Point2D point]
    {
        get => new Array2D<bool>(_states, Size)[point];
        set
        {
            if (_built)
            {
                throw new InvalidOperationException("Builder has already been built.");
            }

            new Array2D<bool>(_states, Size)[point] = value;
        }
    }

    /// <summary>
    /// Builds an immutable RectangularGeneration, transferring ownership of the internal array.
    /// The builder cannot be used after calling Build().
    /// </summary>
    /// <returns>A new RectangularGeneration instance.</returns>
    public IGeneration<Point2D, bool> Build()
    {
        ObjectDisposedException.ThrowIf(_states == null, this);
        if (_built)
        {
            throw new InvalidOperationException("Builder has already been built.");
        }

        _built = true;
        bool[] states = _states;
        _states = null!;

        return new RectangularGeneration(states, Size);
    }

    /// <summary>
    /// Disposes the builder, returning the pooled array if Build() was not called.
    /// </summary>
    public void Dispose()
    {
        if (!_built && _states != null)
        {
            ArrayPool<bool>.Shared.Return(_states);
            _states = null!;
        }
    }

    /// <summary>
    /// An immutable array-backed generation optimized for 2D grid topologies.
    /// Provides O(1) index lookup without dictionary hash overhead.
    /// Owns a pooled array that must be returned via Dispose().
    /// Use RectangularGenerationBuilder to create instances.
    /// </summary>
    private sealed class RectangularGeneration : IGeneration<Point2D, bool>, IDisposable
    {
        private bool[] _states;

        /// <summary>
        /// Creates a new array-backed generation from a pooled array (takes ownership).
        /// </summary>
        public RectangularGeneration(bool[] states, Size2D size)
        {
            _states = states;
            Size = size;
        }

        /// <summary>
        /// Gets the size of the grid.
        /// </summary>
        public Size2D Size { get; }

        /// <summary>
        /// Gets the state of a node.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are out of bounds.</exception>
        public bool this[Point2D node] => new ReadOnlyArray2D<bool>(_states, Size)[node];

        /// <summary>
        /// Disposes the generation, returning the pooled array.
        /// </summary>
        public void Dispose()
        {
            if (_states != null)
            {
                ArrayPool<bool>.Shared.Return(_states);
                _states = null!;
            }
        }
    }
}
