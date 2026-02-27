using System.CommandLine;

using GameOfLife.Console;

string patternsDirectory = Path.Combine(AppContext.BaseDirectory, "patterns");
string hexPatternsDirectory = Path.Combine(AppContext.BaseDirectory, "hex-patterns");

RootCommand rootCommand = CommandLineParser.CreateRootCommand(async options =>
{
    var shapeLoader = new ShapeLoader(patternsDirectory);
    var hexShapeLoader = new HexShapeLoader(hexPatternsDirectory);
    var controller = new GameController(options, shapeLoader, hexShapeLoader, Console.Out, Console.In);
    return await controller.RunAsync();
});

return await rootCommand.Parse(args).InvokeAsync();
