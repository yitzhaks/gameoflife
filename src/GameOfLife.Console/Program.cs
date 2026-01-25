using System.Globalization;

using GameOfLife.Core;
using GameOfLife.Core.Seeds;

const int RequiredArgsCount = 3;

if (args.Length < RequiredArgsCount)
{
    ShowUsage();
    return;
}

if (!int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) || width <= 0)
{
    Console.WriteLine("Invalid width.");
    ShowUsage();
    return;
}

if (!int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height) || height <= 0)
{
    Console.WriteLine("Invalid height.");
    ShowUsage();
    return;
}

var seeds = new List<SeedInput>();
for (var i = 2; i < args.Length; i++)
{
    if (!TryParseSeedArg(args[i], out var seed))
    {
        Console.WriteLine($"Invalid seed format: {args[i]}");
        ShowUsage();
        return;
    }

    seeds.Add(seed);
}

if (seeds.Count == 0)
{
    Console.WriteLine("At least one seed is required.");
    ShowUsage();
    return;
}

var topology = new GridTopology(width, height);
var aliveCells = new HashSet<Point2D>();
var contentRoot = Path.Combine(AppContext.BaseDirectory, "content");
var contentRootFull = Path.GetFullPath(contentRoot);

foreach (var seed in seeds)
{
    var seedPath = Path.GetFullPath(Path.Combine(contentRoot, seed.FileName));
    if (!IsUnderDirectory(contentRootFull, seedPath))
    {
        Console.WriteLine($"Seed file must be under content folder: {seed.FileName}");
        ShowUsage();
        return;
    }

    if (!File.Exists(seedPath))
    {
        Console.WriteLine($"Seed file not found: {seed.FileName}");
        ShowUsage();
        return;
    }

    var pattern = SeedPatternLoader.LoadFromFile(seedPath);
    foreach (var cell in SeedInjection.Apply(pattern, new Point2D(seed.OffsetX, seed.OffsetY), width, height))
    {
        aliveCells.Add(cell);
    }
}

var generation = new DenseGeneration(width, height, aliveCells);
var world = new World<Point2D, bool>(topology, new ConwayRules());
var timeline = new Timeline<Point2D, bool>(world, generation);

Console.Clear();
Render(width, height, timeline.Current);
Console.WriteLine();
Console.WriteLine("Press Right Arrow to advance. Press Esc to exit.");

while (true)
{
    var key = Console.ReadKey(true);
    if (key.Key == ConsoleKey.Escape)
    {
        return;
    }

    if (key.Key != ConsoleKey.RightArrow)
    {
        continue;
    }

    timeline.Step();
    Console.Clear();
    Render(width, height, timeline.Current);
    Console.WriteLine();
    Console.WriteLine("Press Right Arrow to advance. Press Esc to exit.");
}

static void ShowUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  GameOfLife.Console <width> <height> <seedFile@offsetX,offsetY> [seedFile@offsetX,offsetY ...]");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  GameOfLife.Console 20 10 glider.txt@1,1 blinker.txt@10,4");
}

static bool TryParseSeedArg(string value, out SeedInput seed)
{
    seed = default;
    var separatorIndex = value.IndexOf('@');
    if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
    {
        return false;
    }

    var fileName = value[..separatorIndex];
    var offsetPart = value[(separatorIndex + 1)..];
    var offsets = offsetPart.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (offsets.Length != 2)
    {
        return false;
    }

    if (!int.TryParse(offsets[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var offsetX))
    {
        return false;
    }

    if (!int.TryParse(offsets[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var offsetY))
    {
        return false;
    }

    seed = new SeedInput(fileName, offsetX, offsetY);
    return true;
}

static bool IsUnderDirectory(string rootPath, string filePath)
{
    if (string.Equals(rootPath, filePath, StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    if (!rootPath.EndsWith(Path.DirectorySeparatorChar))
    {
        rootPath += Path.DirectorySeparatorChar;
    }

    return filePath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase);
}

static void Render(int width, int height, IGeneration<Point2D, bool> generation)
{
    var horizontalBorder = new string('-', width * 2 + 1);
    Console.WriteLine(horizontalBorder);
    for (var y = 0; y < height; y++)
    {
        Console.Write("|");
        for (var x = 0; x < width; x++)
        {
            var cell = generation[new Point2D(x, y)];
            Console.Write(cell ? '#' : '.');
            Console.Write("|");
        }

        Console.WriteLine();
        Console.WriteLine(horizontalBorder);
    }
}

readonly record struct SeedInput(string FileName, int OffsetX, int OffsetY);
