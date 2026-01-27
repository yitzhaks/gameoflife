using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class ConsoleThemeTests
{
    [Fact]
    public void Default_ReturnsConsistentInstance()
    {
        ConsoleTheme instance1 = ConsoleTheme.Default;
        ConsoleTheme instance2 = ConsoleTheme.Default;

        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void Default_HasExpectedAliveChar()
    {
        ConsoleTheme theme = ConsoleTheme.Default;

        theme.AliveChar.ShouldBe('█');
    }

    [Fact]
    public void Default_HasExpectedDeadChar()
    {
        ConsoleTheme theme = ConsoleTheme.Default;

        theme.DeadChar.ShouldBe('·');
    }

    [Fact]
    public void Default_HasExpectedColors()
    {
        ConsoleTheme theme = ConsoleTheme.Default;

        theme.AliveColor.ShouldBe(ConsoleColor.Green);
        theme.DeadColor.ShouldBe(ConsoleColor.DarkGray);
        theme.BorderColor.ShouldBe(ConsoleColor.Gray);
        theme.ViewportBorderColor.ShouldBe(ConsoleColor.DarkGray);
    }

    [Fact]
    public void Default_ShowBorderIsTrue()
    {
        ConsoleTheme theme = ConsoleTheme.Default;

        theme.ShowBorder.ShouldBeTrue();
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

        theme.AliveChar.ShouldBe('O');
        theme.DeadChar.ShouldBe('-');
        theme.AliveColor.ShouldBe(ConsoleColor.Yellow);
        theme.DeadColor.ShouldBe(ConsoleColor.Black);
        theme.BorderColor.ShouldBe(ConsoleColor.White);
        theme.ViewportBorderColor.ShouldBe(ConsoleColor.Cyan);
        theme.ShowBorder.ShouldBeFalse();
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var theme1 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var theme2 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        theme1.ShouldBe(theme2);
        (theme1 == theme2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var theme1 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var theme2 = new ConsoleTheme(AliveChar: 'O', DeadChar: '.', ShowBorder: true);

        theme1.ShouldNotBe(theme2);
        (theme1 != theme2).ShouldBeTrue();
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var theme1 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        var theme2 = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        theme1.GetHashCode().ShouldBe(theme2.GetHashCode());
    }

    [Fact]
    public void ToString_ContainsPropertyValues()
    {
        var theme = new ConsoleTheme(AliveChar: 'O', DeadChar: '-', ShowBorder: false);

        string result = theme.ToString();

        result.ShouldContain("O");
        result.ShouldContain("-");
    }

    [Fact]
    public void With_ModifiesSpecificProperty()
    {
        var original = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);
        ConsoleTheme modified = original with { AliveChar = 'O' };

        modified.AliveChar.ShouldBe('O');
        modified.DeadChar.ShouldBe('.');
        modified.ShowBorder.ShouldBeTrue();
    }
}
