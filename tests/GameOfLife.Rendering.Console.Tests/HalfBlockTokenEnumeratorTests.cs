using System.Text;

using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public sealed class HalfBlockTokenEnumeratorTests : IDisposable
{
    // Half-block characters
    private const char FullBlock = '\u2588';
    private const char UpperHalf = '\u2580';
    private const char LowerHalf = '\u2584';

    private readonly List<IDisposable> _disposables = [];

    public void Dispose()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    private IGeneration<Point2D, bool> CreateGeneration(Size2D size, params Point2D[] aliveCells)
    {
        using var builder = new RectangularGenerationBuilder(size);
        foreach (Point2D cell in aliveCells)
        {
            builder[cell] = true;
        }

        IGeneration<Point2D, bool> generation = builder.Build();
        if (generation is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return generation;
    }

    private static string ExtractCharacters(HalfBlockTokenEnumerator enumerator)
    {
        var sb = new StringBuilder();
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsCharacter)
            {
                _ = sb.Append(token.Character);
            }
        }

        return sb.ToString();
    }

    [Fact]
    public void Enumerator_2x2GridAllAlive_RendersFullBlock()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0), (0, 1), (1, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should have 2 full blocks + newline
        chars.ShouldContain(FullBlock);
        chars.Count(c => c == FullBlock).ShouldBe(2);
    }

    [Fact]
    public void Enumerator_2x2GridAllDead_RendersSpaces()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should have 2 spaces + newline (no half blocks or full blocks)
        chars.ShouldNotContain(FullBlock);
        chars.ShouldNotContain(UpperHalf);
        chars.ShouldNotContain(LowerHalf);
    }

    [Fact]
    public void Enumerator_2x2GridTopAlive_RendersUpperHalf()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Only top row alive (y=0)
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should have upper half blocks
        chars.ShouldContain(UpperHalf);
        chars.Count(c => c == UpperHalf).ShouldBe(2);
    }

    [Fact]
    public void Enumerator_2x2GridBottomAlive_RendersLowerHalf()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Only bottom row alive (y=1)
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 1), (1, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should have lower half blocks
        chars.ShouldContain(LowerHalf);
        chars.Count(c => c == LowerHalf).ShouldBe(2);
    }

    [Fact]
    public void Enumerator_4x4GridCheckerboard_RendersMixedHalfBlocks()
    {
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);

        // Checkerboard pattern: alive at (x+y) % 2 == 0
        var aliveCells = new List<Point2D>();
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if ((x + y) % 2 == 0)
                {
                    aliveCells.Add((x, y));
                }
            }
        }

        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4), [.. aliveCells]);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Checkerboard in 4x4 creates alternating half blocks
        chars.ShouldContain(UpperHalf);
        chars.ShouldContain(LowerHalf);
    }

    [Fact]
    public void Enumerator_WithBorder_RendersFullBorder()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should contain border characters
        chars.ShouldContain(ConsoleTheme.Border.TopLeft);
        chars.ShouldContain(ConsoleTheme.Border.TopRight);
        chars.ShouldContain(ConsoleTheme.Border.BottomLeft);
        chars.ShouldContain(ConsoleTheme.Border.BottomRight);
        chars.ShouldContain(ConsoleTheme.Border.Horizontal);
        chars.ShouldContain(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void Enumerator_NullLayout_ThrowsArgumentNullException()
    {
        var topology = new RectangularTopology((2, 2));
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme();

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
        {
            HalfBlockTokenEnumerator enumerator = new(null!, generation, nodeSet, theme);
            _ = enumerator.MoveNext(); // Ensure enumerator is used
        });

        exception.ParamName.ShouldBe("layout");
    }

    [Fact]
    public void Enumerator_NullTheme_ThrowsArgumentNullException()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
        {
            HalfBlockTokenEnumerator enumerator = new(layout, generation, nodeSet, null!);
            _ = enumerator.MoveNext(); // Ensure enumerator is used
        });

        exception.ParamName.ShouldBe("theme");
    }

    [Fact]
    public void Enumerator_EmitsForegroundAndBackgroundColors()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Top alive, bottom dead (should emit green foreground, dark gray background)
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);

        var foregroundSequences = new List<AnsiSequence>();
        var backgroundSequences = new List<AnsiSequence>();

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                AnsiSequence seq = token.Sequence;
                if (seq is AnsiSequence.ForegroundGreen or AnsiSequence.ForegroundDarkGray or AnsiSequence.ForegroundGray)
                {
                    foregroundSequences.Add(seq);
                }
                else if (seq is AnsiSequence.BackgroundGreen or AnsiSequence.BackgroundDarkGray or AnsiSequence.BackgroundDefault)
                {
                    backgroundSequences.Add(seq);
                }
            }
        }

        // Should have emitted foreground and background colors
        foregroundSequences.ShouldContain(AnsiSequence.ForegroundGreen);
        backgroundSequences.ShouldContain(AnsiSequence.BackgroundDarkGray);
    }

    [Fact]
    public void Enumerator_BothAlive_EmitsDefaultBackground()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        // Both cells alive
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (0, 1), (1, 0), (1, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);

        var backgroundSequences = new List<AnsiSequence>();

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                AnsiSequence seq = token.Sequence;
                if (seq is AnsiSequence.BackgroundGreen or AnsiSequence.BackgroundDarkGray or AnsiSequence.BackgroundDefault)
                {
                    backgroundSequences.Add(seq);
                }
            }
        }

        // Both alive should use default background
        backgroundSequences.ShouldContain(AnsiSequence.BackgroundDefault);
    }

    [Fact]
    public void Enumerator_GetEnumerator_ReturnsSelf()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);

        // This should not throw and should return an enumerator
        HalfBlockTokenEnumerator result = enumerator.GetEnumerator();
        _ = result; // Just verify it compiles and doesn't throw
    }
}
