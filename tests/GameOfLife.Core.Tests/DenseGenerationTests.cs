namespace GameOfLife.Core.Tests;

public class DenseGenerationTests
{
    [Fact]
    public void Constructor_InvalidWidth_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new DenseGeneration(0, 1));

        Assert.Equal("width", exception.ParamName);
    }

    [Fact]
    public void Constructor_InvalidHeight_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new DenseGeneration(1, 0));

        Assert.Equal("height", exception.ParamName);
    }

    [Fact]
    public void Indexer_UnknownNode_Throws()
    {
        var generation = new DenseGeneration(2, 2);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => generation[new Point2D(3, 3)]);

        Assert.Equal("node", exception.ParamName);
    }

    [Fact]
    public void Indexer_KnownNode_ReturnsState()
    {
        var generation = new DenseGeneration(2, 2, [new Point2D(1, 1)]);

        Assert.True(generation[new Point2D(1, 1)]);
        Assert.False(generation[new Point2D(0, 0)]);
    }
}
