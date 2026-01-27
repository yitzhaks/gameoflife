namespace GameOfLife.Console;

/// <summary>
/// Command-line options for the Game of Life console application.
/// </summary>
internal sealed class CommandLineOptions
{
    /// <summary>
    /// Gets or sets the width of the grid.
    /// </summary>
    public int Width { get; set; } = 20;

    /// <summary>
    /// Gets or sets the height of the grid.
    /// </summary>
    public int Height { get; set; } = 20;

    /// <summary>
    /// Gets or sets the shape injections to apply at startup.
    /// </summary>
    public List<ShapeInjection> Injections { get; set; } = [];

    /// <summary>
    /// Gets or sets the maximum number of generations to run (null = unlimited).
    /// </summary>
    public int? MaxGenerations { get; set; }

    /// <summary>
    /// Gets or sets whether to start in autoplay mode.
    /// </summary>
    public bool StartAutoplay { get; set; }

    /// <summary>
    /// Gets or sets the maximum frames per second during autoplay (default: 30).
    /// </summary>
    public int MaxFps { get; set; } = 30;
}
