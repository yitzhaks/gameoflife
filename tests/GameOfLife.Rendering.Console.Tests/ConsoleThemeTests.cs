using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ConsoleThemeTests
{
    [Fact]
    public void Default_ReturnsConsistentInstance()
    {
        ConsoleTheme instance1 = ConsoleTheme.Default;
        ConsoleTheme instance2 = ConsoleTheme.Default;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Default_HasExpectedAliveChar()
    {
        ConsoleTheme theme = ConsoleTheme.Default;

        Assert.Equal('█', theme.AliveChar);
    }

    [Fact]
    public void Default_HasExpectedDeadChar()
    {
        ConsoleTheme theme = ConsoleTheme.Default;

        Assert.Equal('·', theme.DeadChar);
    }

    [Fact]
    public void Default_HasExpectedColors()
    {
        ConsoleTheme theme = ConsoleTheme.Default;

        Assert.Equal(ConsoleColor.Green, theme.AliveColor);
        Assert.Equal(ConsoleColor.DarkGray, theme.DeadColor);
        Assert.Equal(ConsoleColor.Gray, theme.BorderColor);
        Assert.Equal(ConsoleColor.DarkGray, theme.ViewportBorderColor);
    }

    [Fact]
    public void Default_ShowBorderIsTrue()
    {
        ConsoleTheme theme = ConsoleTheme.Default;

        Assert.True(theme.ShowBorder);
    }

    [Fact]
    public void Constructor_WithCustomValues_SetsAllProperties()
    {
        var theme = new ConsoleTheme(
            AliveChar: 'O',
            DeadChar: '-',
            AliveColor: ConsoleColor.Yellow,
            DeadColor: ConsoleColor.Black,
            BorderColor: ConsoleColor.White,
            ViewportBorderColor: ConsoleColor.Cyan,
            ShowBorder: false);

        Assert.Equal('O', theme.AliveChar);
        Assert.Equal('-', theme.DeadChar);
        Assert.Equal(ConsoleColor.Yellow, theme.AliveColor);
        Assert.Equal(ConsoleColor.Black, theme.DeadColor);
        Assert.Equal(ConsoleColor.White, theme.BorderColor);
        Assert.Equal(ConsoleColor.Cyan, theme.ViewportBorderColor);
        Assert.False(theme.ShowBorder);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var theme1 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var theme2 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        Assert.Equal(theme1, theme2);
        Assert.True(theme1 == theme2);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var theme1 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var theme2 = new ConsoleTheme(AliveChar: 'O', DeadChar: '.', ShowBorder: true);

        Assert.NotEqual(theme1, theme2);
        Assert.True(theme1 != theme2);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var theme1 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var theme2 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        Assert.Equal(theme1.GetHashCode(), theme2.GetHashCode());
    }

    [Fact]
    public void ToString_ContainsPropertyValues()
    {
        var theme = new ConsoleTheme(AliveChar: 'O', DeadChar: '-', ShowBorder: false);

        string result = theme.ToString();

        Assert.Contains("O", result);
        Assert.Contains("-", result);
    }

    [Fact]
    public void With_ModifiesSpecificProperty()
    {
        var original = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        ConsoleTheme modified = original with { AliveChar = 'O' };

        Assert.Equal('O', modified.AliveChar);
        Assert.Equal('.', modified.DeadChar);
        Assert.True(modified.ShowBorder);
    }
}
