using GameOfLife.Console;

var patternsDirectory = Path.Combine(AppContext.BaseDirectory, "patterns");

var rootCommand = CommandLineParser.CreateRootCommand(async options =>
{
    var shapeLoader = new ShapeLoader(patternsDirectory);
    var controller = new GameController(options, shapeLoader, Console.Out, Console.In);
    return await controller.RunAsync();
});

return await rootCommand.Parse(args).InvokeAsync();
