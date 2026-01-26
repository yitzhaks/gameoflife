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
    /// Gets or sets the delay between generations in milliseconds (auto mode).
    /// </summary>
    public int Delay { get; set; } = 200;

    /// <summary>
    /// Gets or sets whether to run in automatic mode.
    /// </summary>
    public bool Auto { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of generations to run (null = unlimited).
    /// </summary>
    public int? MaxGenerations { get; set; } = null;
}
