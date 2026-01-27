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

    #region Corner Character Selection Tests

    [Fact]
    public void GetTopLeftCorner_AtTopAndLeft_ReturnsTopLeftCorner()
    {
        // Viewport at top-left corner: isAtTop=true, isAtLeft=true
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 4, 2); // At top-left, small viewport

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.TopLeft);
    }

    [Fact]
    public void GetTopLeftCorner_AtTopNotAtLeft_ReturnsHorizontal()
    {
        // Viewport at top but not left: isAtTop=true, isAtLeft=false
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 6, 2);
        viewport.Move(2, 0); // Move right so not at left edge

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        // First character after color should be horizontal (not corner)
        // The top-left position should be horizontal since we're not at left edge
        chars.ShouldContain(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetTopLeftCorner_NotAtTopAtLeft_ReturnsVertical()
    {
        // Viewport not at top but at left: isAtTop=false, isAtLeft=true
        var topology = new RectangularTopology((4, 6));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 4, 3);
        viewport.Move(0, 1); // Move down so not at top edge

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        // Should have vertical bar in first column position
        chars.ShouldContain(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetTopLeftCorner_NotAtTopNotAtLeft_ReturnsDiagonalTopLeft()
    {
        // Viewport not at top and not at left: isAtTop=false, isAtLeft=false
        var topology = new RectangularTopology((8, 8));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((8, 8));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 8, 4);
        viewport.Move(2, 1); // Move right and down so not at top-left

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.ViewportBorder.DiagonalTopLeft);
    }

    [Fact]
    public void GetTopRightCorner_AtTopAndRight_ReturnsTopRightCorner()
    {
        // Viewport at top-right corner: isAtTop=true, isAtRight=true
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 4, 2);
        viewport.Move(10, 0); // Move right to reach right edge (clamped)

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.TopRight);
    }

    [Fact]
    public void GetTopRightCorner_AtTopNotAtRight_ReturnsHorizontal()
    {
        // Viewport at top but not right: isAtTop=true, isAtRight=false
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 6, 2); // At top-left, doesn't reach right

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        // Top-right corner should be horizontal when not at right edge
        chars.ShouldContain(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetTopRightCorner_NotAtTopAtRight_ReturnsVertical()
    {
        // Viewport not at top but at right: isAtTop=false, isAtRight=true
        var topology = new RectangularTopology((4, 6));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(4, 2, 4, 3); // Full width viewport
        viewport.Move(0, 1); // Move down so not at top

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetTopRightCorner_NotAtTopNotAtRight_ReturnsDiagonalTopRight()
    {
        // Viewport not at top and not at right: isAtTop=false, isAtRight=false
        var topology = new RectangularTopology((8, 8));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((8, 8));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 8, 4);
        viewport.Move(0, 1); // Move down, at left edge but not top

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.ViewportBorder.DiagonalTopRight);
    }

    [Fact]
    public void GetBottomLeftCorner_AtBottomAndLeft_ReturnsBottomLeftCorner()
    {
        // Viewport at bottom-left corner: isAtBottom=true, isAtLeft=true
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 4, 2);
        viewport.Move(0, 10); // Move down to reach bottom (clamped)

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.BottomLeft);
    }

    [Fact]
    public void GetBottomLeftCorner_AtBottomNotAtLeft_ReturnsHorizontal()
    {
        // Viewport at bottom but not left: isAtBottom=true, isAtLeft=false
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 6, 2);
        viewport.Move(2, 10); // Move right and down to bottom but not left

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetBottomLeftCorner_NotAtBottomAtLeft_ReturnsVertical()
    {
        // Viewport not at bottom but at left: isAtBottom=false, isAtLeft=true
        var topology = new RectangularTopology((4, 6));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 4, 3); // At left, doesn't reach bottom

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetBottomLeftCorner_NotAtBottomNotAtLeft_ReturnsDiagonalBottomLeft()
    {
        // Viewport not at bottom and not at left: isAtBottom=false, isAtLeft=false
        var topology = new RectangularTopology((8, 8));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((8, 8));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 8, 4);
        viewport.Move(2, 0); // Move right so not at left, still at top

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.ViewportBorder.DiagonalBottomLeft);
    }

    [Fact]
    public void GetBottomRightCorner_AtBottomAndRight_ReturnsBottomRightCorner()
    {
        // Viewport at bottom-right corner: isAtBottom=true, isAtRight=true
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(4, 2, 4, 2); // Full width, full height

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.BottomRight);
    }

    [Fact]
    public void GetBottomRightCorner_AtBottomNotAtRight_ReturnsHorizontal()
    {
        // Viewport at bottom but not right: isAtBottom=true, isAtRight=false
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 6, 2); // At bottom-left

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void GetBottomRightCorner_NotAtBottomAtRight_ReturnsVertical()
    {
        // Viewport not at bottom but at right: isAtBottom=false, isAtRight=true
        var topology = new RectangularTopology((4, 6));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(4, 2, 4, 3); // Full width, doesn't reach bottom

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void GetBottomRightCorner_NotAtBottomNotAtRight_ReturnsDiagonalBottomRight()
    {
        // Viewport not at bottom and not at right: isAtBottom=false, isAtRight=false
        var topology = new RectangularTopology((8, 8));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((8, 8));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 8, 4); // Small viewport, in center

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.ViewportBorder.DiagonalBottomRight);
    }

    #endregion

    #region Viewport Border Arrow Tests

    [Fact]
    public void TopBorder_NotAtTop_RendersUpArrows()
    {
        var topology = new RectangularTopology((4, 6));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(4, 2, 4, 3);
        viewport.Move(0, 1); // Move down so not at top

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.ViewportBorder.Up);
    }

    [Fact]
    public void BottomBorder_NotAtBottom_RendersDownArrows()
    {
        var topology = new RectangularTopology((4, 6));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(4, 2, 4, 3); // At top, doesn't reach bottom

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.ViewportBorder.Down);
    }

    [Fact]
    public void LeftBorder_NotAtLeft_RendersLeftArrows()
    {
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 6, 2);
        viewport.Move(2, 0); // Move right so not at left

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.ViewportBorder.Left);
    }

    [Fact]
    public void RightBorder_NotAtRight_RendersRightArrows()
    {
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 6, 2); // At left, doesn't reach right

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldContain(ConsoleTheme.ViewportBorder.Right);
    }

    #endregion

    #region Border Color Tests

    [Fact]
    public void TopBorder_NotAtTop_EmitsDarkGrayColor()
    {
        var topology = new RectangularTopology((4, 6));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(4, 2, 4, 3);
        viewport.Move(0, 1); // Move down so not at top

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);

        var foregroundSequences = new List<AnsiSequence>();
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence && token.Sequence == AnsiSequence.ForegroundDarkGray)
            {
                foregroundSequences.Add(token.Sequence);
            }
        }

        foregroundSequences.ShouldContain(AnsiSequence.ForegroundDarkGray);
    }

    [Fact]
    public void LeftBorder_NotAtLeft_EmitsDarkGrayColor()
    {
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 6, 2);
        viewport.Move(2, 0); // Move right so not at left

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);

        var foregroundSequences = new List<AnsiSequence>();
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence && token.Sequence == AnsiSequence.ForegroundDarkGray)
            {
                foregroundSequences.Add(token.Sequence);
            }
        }

        foregroundSequences.ShouldContain(AnsiSequence.ForegroundDarkGray);
    }

    [Fact]
    public void RightBorder_NotAtRight_EmitsDarkGrayColor()
    {
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(2, 2, 6, 2); // At left, doesn't reach right

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);

        var foregroundSequences = new List<AnsiSequence>();
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence && token.Sequence == AnsiSequence.ForegroundDarkGray)
            {
                foregroundSequences.Add(token.Sequence);
            }
        }

        foregroundSequences.ShouldContain(AnsiSequence.ForegroundDarkGray);
    }

    [Fact]
    public void BottomBorder_NotAtBottom_EmitsDarkGrayColor()
    {
        var topology = new RectangularTopology((4, 6));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(4, 2, 4, 3); // At top, doesn't reach bottom

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);

        var foregroundSequences = new List<AnsiSequence>();
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence && token.Sequence == AnsiSequence.ForegroundDarkGray)
            {
                foregroundSequences.Add(token.Sequence);
            }
        }

        foregroundSequences.ShouldContain(AnsiSequence.ForegroundDarkGray);
    }

    #endregion

    #region ShowBorder=false Path Tests

    [Fact]
    public void Enumerator_ShowBorderFalse_SkipsLeftBorder()
    {
        var topology = new RectangularTopology((2, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should not contain any border characters
        chars.ShouldNotContain(ConsoleTheme.Border.Vertical);
        chars.ShouldNotContain(ConsoleTheme.Border.TopLeft);
        chars.ShouldNotContain(ConsoleTheme.Border.TopRight);
        chars.ShouldNotContain(ConsoleTheme.Border.BottomLeft);
        chars.ShouldNotContain(ConsoleTheme.Border.BottomRight);
        chars.ShouldNotContain(ConsoleTheme.Border.Horizontal);
    }

    [Fact]
    public void Enumerator_ShowBorderFalse_SkipsRightBorder()
    {
        var topology = new RectangularTopology((2, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should not contain vertical borders (used for left and right)
        chars.ShouldNotContain(ConsoleTheme.Border.Vertical);
    }

    [Fact]
    public void Enumerator_ShowBorderFalse_SkipsTopBorder()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldNotContain(ConsoleTheme.Border.TopLeft);
        chars.ShouldNotContain(ConsoleTheme.Border.TopRight);
    }

    [Fact]
    public void Enumerator_ShowBorderFalse_SkipsBottomBorder()
    {
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        chars.ShouldNotContain(ConsoleTheme.Border.BottomLeft);
        chars.ShouldNotContain(ConsoleTheme.Border.BottomRight);
    }

    [Fact]
    public void Enumerator_ShowBorderFalse_MultipleRows_TransitionsCorrectly()
    {
        // Test that multiple rows work correctly without borders
        var topology = new RectangularTopology((4, 6)); // 3 output rows
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should have exactly 3 newlines (one per row)
        int newlineCount = chars.Count(c => c == '\n');
        newlineCount.ShouldBe(3);
    }

    #endregion

    #region Viewport Clipping Tests

    [Fact]
    public void Enumerator_ViewportClipsContent_RendersOnlyVisibleCells()
    {
        var topology = new RectangularTopology((6, 6));
        var layout = new HalfBlockLayout(topology);
        // Create alive cells in a pattern
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 6), (0, 0), (5, 5));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);
        var viewport = new Viewport(2, 2, 6, 3); // View only partial content

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        // Should render only 2 columns worth of cells plus newlines
        // 2 cells per row, 2 rows = 4 cell characters + 2 newlines
        int nonNewlineChars = chars.Count(c => c != '\n');
        nonNewlineChars.ShouldBe(4); // 2 columns * 2 rows
    }

    [Fact]
    public void Enumerator_ViewportWithBorder_RendersCorrectWidth()
    {
        var topology = new RectangularTopology((6, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);
        var viewport = new Viewport(3, 2, 6, 2); // 3-column viewport

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        // Count characters in first line (before first newline)
        int firstNewline = chars.IndexOf('\n');
        int firstLineLength = firstNewline;

        // Should be: 1 left border + 3 cells + 1 right border = 5 characters
        firstLineLength.ShouldBe(5);
    }

    [Fact]
    public void Enumerator_ViewportAtEdge_ClampsBoundsCorrectly()
    {
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);
        // Viewport larger than what's available after offset
        var viewport = new Viewport(10, 10, 4, 2); // Request 10x10 but only 4x2 available
        viewport.Move(0, 0); // At origin

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        // Should still render successfully with clamped bounds
        chars.ShouldNotBeEmpty();
    }

    #endregion

    #region Color Optimization Tests

    [Fact]
    public void Enumerator_SameForegroundColor_DoesNotEmitDuplicateColorCodes()
    {
        var topology = new RectangularTopology((4, 2));
        var layout = new HalfBlockLayout(topology);
        // All cells dead - same color throughout
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);

        int darkGrayForegroundCount = 0;
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence && token.Sequence == AnsiSequence.ForegroundDarkGray)
            {
                darkGrayForegroundCount++;
            }
        }

        // Should only emit color once since all cells are the same
        darkGrayForegroundCount.ShouldBe(1);
    }

    [Fact]
    public void Enumerator_SameBackgroundColor_DoesNotEmitDuplicateColorCodes()
    {
        var topology = new RectangularTopology((4, 2));
        var layout = new HalfBlockLayout(topology);
        // All cells dead - same background throughout
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 2));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);

        int darkGrayBackgroundCount = 0;
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence && token.Sequence == AnsiSequence.BackgroundDarkGray)
            {
                darkGrayBackgroundCount++;
            }
        }

        // Should only emit background color once since all cells are the same
        darkGrayBackgroundCount.ShouldBe(1);
    }

    #endregion

    #region Topology Boundary Tests

    [Fact]
    public void Enumerator_CellsOutsideTopology_RendersAsSpace()
    {
        // Create topology but with sparse node set
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4), (1, 1));

        // Create a nodeSet that excludes some nodes
        var sparseNodeSet = new HashSet<Point2D>
        {
            (1, 0), (1, 1), (2, 0), (2, 1) // Only middle columns
        };

        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet: sparseNodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should contain spaces for cells outside the nodeSet
        chars.ShouldContain(' ');
    }

    #endregion

    #region Row Transition and Boundary Tests

    [Fact]
    public void Enumerator_MultipleRowsNoBorder_TransitionsCorrectlyBetweenRows()
    {
        // Test the row newline transition without borders
        var topology = new RectangularTopology((3, 6)); // 3 packed rows
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((3, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should have 3 newlines (one per packed row)
        int newlineCount = chars.Count(c => c == '\n');
        newlineCount.ShouldBe(3);
    }

    [Fact]
    public void Enumerator_MultipleRowsWithBorder_TransitionsCorrectlyBetweenRows()
    {
        // Test the row newline transition with borders
        var topology = new RectangularTopology((3, 6)); // 3 packed rows
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((3, 6));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should contain border elements
        chars.ShouldContain(ConsoleTheme.Border.TopLeft);
        chars.ShouldContain(ConsoleTheme.Border.BottomLeft);

        // Should have 5 newlines: top border + 3 content rows + bottom border
        int newlineCount = chars.Count(c => c == '\n');
        newlineCount.ShouldBe(5);
    }

    [Fact]
    public void Enumerator_SingleCellGrid_RendersCorrectly()
    {
        var topology = new RectangularTopology((1, 2)); // Single packed cell
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((1, 2), (0, 0), (0, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should have full block for single cell with both alive
        chars.ShouldContain(FullBlock);
    }

    [Fact]
    public void Enumerator_ViewportLargerThanBoard_RendersFullBoard()
    {
        // Test when viewport is larger than the board
        var topology = new RectangularTopology((4, 4)); // 2 packed rows
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        // Viewport larger than board (10x10 viewport for 4x2 packed board)
        var viewport = new Viewport(10, 10, 4, 2);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        string[] lines = chars.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should render full 4x2 packed grid (clamped to board bounds)
        lines.Length.ShouldBe(2);
        foreach (string line in lines)
        {
            line.Length.ShouldBe(4);
        }
    }

    [Fact]
    public void Enumerator_ViewportMovedToRightEdge_RendersCorrectly()
    {
        // Test when viewport is moved to the right edge of the board
        var topology = new RectangularTopology((6, 4)); // 6 columns, 2 packed rows
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((6, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        // 4x2 viewport moved to right edge (OffsetX clamped to 2)
        var viewport = new Viewport(4, 2, 6, 2);
        viewport.Move(100, 0); // Move far right, will be clamped

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport);
        string chars = ExtractCharacters(enumerator);

        string[] lines = chars.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should render 4 columns x 2 packed rows
        lines.Length.ShouldBe(2);
        foreach (string line in lines)
        {
            line.Length.ShouldBe(4);
        }
    }

    #endregion

    #region Mixed Cell State Tests

    [Fact]
    public void Enumerator_OnlyTopCellInTopology_RendersUpperHalfOrSpace()
    {
        // Test when only top cell is in topology (bottom not in topology)
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 0), (1, 0));

        // Only include top row in nodeSet
        var sparseNodeSet = new HashSet<Point2D>
        {
            (0, 0), (1, 0) // Only top row
        };

        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet: sparseNodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should render upper half blocks for top row
        // (top alive, bottom not in topology)
        chars.ShouldContain(UpperHalf);
    }

    [Fact]
    public void Enumerator_OnlyBottomCellInTopology_RendersLowerHalfOrSpace()
    {
        // Test when only bottom cell is in topology (top not in topology)
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (0, 1), (1, 1));

        // Only include bottom row in nodeSet
        var sparseNodeSet = new HashSet<Point2D>
        {
            (0, 1), (1, 1) // Only bottom row
        };

        var theme = new ConsoleTheme(ShowBorder: false);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet: sparseNodeSet, theme);
        string chars = ExtractCharacters(enumerator);

        // Should render lower half blocks for bottom row
        // (top not in topology, bottom alive)
        chars.ShouldContain(LowerHalf);
    }

    [Fact]
    public void Enumerator_ColorTransitionsFromCellToBorder_EmitsBorderColor()
    {
        // Test that border color transitions correctly after cell content
        var topology = new RectangularTopology((2, 2));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((2, 2), (1, 0), (1, 1));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: true);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);

        var colorSequences = new List<AnsiSequence>();
        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            if (token.IsSequence)
            {
                colorSequences.Add(token.Sequence);
            }
        }

        // Should have color transitions including gray for borders
        colorSequences.ShouldContain(AnsiSequence.ForegroundGray);
        colorSequences.ShouldContain(AnsiSequence.ForegroundGreen);
    }

    #endregion

    #region Null Viewport Behavior Tests

    [Fact]
    public void Enumerator_NullViewport_UsesFullBoundsFromLayout()
    {
        var topology = new RectangularTopology((4, 4));
        var layout = new HalfBlockLayout(topology);
        IGeneration<Point2D, bool> generation = CreateGeneration((4, 4));
        var nodeSet = new HashSet<Point2D>(topology.Nodes);
        var theme = new ConsoleTheme(ShowBorder: false);

        // Pass null viewport explicitly
        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme, viewport: null);
        string chars = ExtractCharacters(enumerator);

        string[] lines = chars.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should render full 4x2 packed grid
        lines.Length.ShouldBe(2);
        foreach (string line in lines)
        {
            line.Length.ShouldBe(4);
        }
    }

    #endregion
}
