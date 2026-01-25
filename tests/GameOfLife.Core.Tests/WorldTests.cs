namespace GameOfLife.Core.Tests;

public class WorldTests
{
    [Fact]
    public void Tick_GridTopology_ReturnsDenseGeneration()
    {
        var topology = new GridTopology(2, 2);
        var world = new World<Point2D, bool>(topology, new ConwayRules());
        var generation = new DenseGeneration(2, 2, [new Point2D(0, 0)]);

        var next = world.Tick(generation);

        Assert.IsType<DenseGeneration>(next);
    }

    [Fact]
    public void Tick_BlinkerOscillates()
    {
        var topology = new GridTopology(5, 5);
        var world = new World<Point2D, bool>(topology, new ConwayRules());
        var initial = new DenseGeneration(5, 5, [new Point2D(2, 1), new Point2D(2, 2), new Point2D(2, 3)]);

        var next = world.Tick(initial);

        Assert.True(next[new Point2D(1, 2)]);
        Assert.True(next[new Point2D(2, 2)]);
        Assert.True(next[new Point2D(3, 2)]);
        Assert.False(next[new Point2D(2, 1)]);
        Assert.False(next[new Point2D(2, 3)]);
    }
}
