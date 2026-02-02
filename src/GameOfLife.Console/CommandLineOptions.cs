using GameOfLife.Rendering.Console;

namespace GameOfLife.Console;

/// <summary>
/// The type of board topology to use.
/// </summary>
internal enum BoardTopology
{
    /// <summary>
    /// Rectangular grid with 8-neighbor connectivity (Moore neighborhood).
    /// </summary>
    Rectangular,

    /// <summary>
    /// Hexagonal grid with 6-neighbor connectivity.
    /// </summary>
    Hexagonal
}

/// <summary>
/// Command-line options for the Game of Life console application.
/// </summary>
internal sealed class CommandLineOptions
{
    /// <summary>
    /// Gets or sets the width of the grid (for rectangular topology).
    /// </summary>
    public int Width { get; set; } = 20;

    /// <summary>
    /// Gets or sets the height of the grid (for rectangular topology).
    /// </summary>
    public int Height { get; set; } = 20;

    /// <summary>
    /// Gets or sets the board topology type.
    /// </summary>
    public BoardTopology Topology { get; set; } = BoardTopology.Rectangular;

    /// <summary>
    /// Gets or sets the radius for hexagonal topology (default: 10).
    /// </summary>
    public int HexRadius { get; set; } = 10;

    /// <summary>
    /// Gets or sets the percentage of cells to randomly fill for hex boards (0-100, default: 0).
    /// </summary>
    public int HexFillPercent { get; set; }

    /// <summary>
    /// Gets or sets the hex rule set (default: B2S34).
    /// </summary>
    public string HexRules { get; set; } = "B2S34";

    /// <summary>
    /// Gets or sets whether to run pattern analysis instead of the game.
    /// </summary>
    public bool AnalyzePatterns { get; set; }

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

    /// <summary>
    /// Gets or sets the aspect ratio correction mode (default: None).
    /// </summary>
    public AspectMode AspectMode { get; set; } = AspectMode.None;

    /// <summary>
    /// Gets or sets the hex shape injections to apply at startup.
    /// </summary>
    public List<HexShapeInjection> HexInjections { get; set; } = [];
}
