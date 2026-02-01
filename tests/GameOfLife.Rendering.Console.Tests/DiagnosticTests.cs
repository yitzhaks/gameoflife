using System.Text;

using GameOfLife.Core;
using GameOfLife.Rendering;

using Shouldly;

using Xunit;
using Xunit.Abstractions;

namespace GameOfLife.Rendering.Console.Tests;

public class DiagnosticTests
{
    private readonly ITestOutputHelper _output;

    public DiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TokenEnumerator_Diagnostic_ShowsAllTokens()
    {
        var topology = new RectangularTopology((3, 3));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        using var builder = new RectangularGenerationBuilder((3, 3));
        builder[(1, 1)] = true;
        using IGeneration<Point2D, bool> generation = builder.Build();

        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var enumerator = new TokenEnumerator(layout, generation, nodeSet, theme);
        int tokenCount = 0;
        int colorCount = 0;
        var sb = new StringBuilder();

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            tokenCount++;
            if (token.IsSequence)
            {
                colorCount++;
                _ = sb.AppendLine($"Token {tokenCount}: ANSI {token.Sequence}");
            }
            else if (token.Character == '\n')
            {
                _ = sb.AppendLine($"Token {tokenCount}: NEWLINE");
            }
            else
            {
                _ = sb.AppendLine($"Token {tokenCount}: CHAR '{token.Character}'");
            }
        }

        _output.WriteLine(sb.ToString());
        _output.WriteLine($"Total tokens: {tokenCount}, Color tokens: {colorCount}");

        colorCount.ShouldBeGreaterThan(0, "Should have at least one color token");
    }

    [Fact]
    public void FullPipeline_Diagnostic_ShowsGlyphsWithColors()
    {
        var topology = new RectangularTopology((3, 3));
        var engine = new IdentityLayoutEngine();
        ILayout<Point2D, Point2D, RectangularBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        using var builder = new RectangularGenerationBuilder((3, 3));
        builder[(1, 1)] = true;
        using IGeneration<Point2D, bool> generation = builder.Build();

        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        // Go through full pipeline
        var tokenEnumerator = new TokenEnumerator(layout, generation, nodeSet, theme);
        GlyphEnumerator glyphEnumerator = GlyphReader.FromTokens(tokenEnumerator);
        ColorNormalizedGlyphEnumerator normalizedEnumerator = AnsiStateTracker.FromGlyphs(glyphEnumerator);

        int glyphCount = 0;
        int coloredGlyphCount = 0;
        var sb = new StringBuilder();

        while (normalizedEnumerator.MoveNext())
        {
            Glyph glyph = normalizedEnumerator.Current;
            glyphCount++;
            if (glyph.Color.HasValue)
            {
                coloredGlyphCount++;
                _ = sb.AppendLine($"Glyph {glyphCount}: '{(glyph.IsNewline ? "\\n" : glyph.Character.ToString())}' Color={glyph.Color}");
            }
            else
            {
                _ = sb.AppendLine($"Glyph {glyphCount}: '{(glyph.IsNewline ? "\\n" : glyph.Character.ToString())}' Color=null");
            }
        }

        _output.WriteLine(sb.ToString());
        _output.WriteLine($"Total glyphs: {glyphCount}, Colored glyphs: {coloredGlyphCount}");

        coloredGlyphCount.ShouldBeGreaterThan(0, "Should have at least one colored glyph");
    }

    [Fact]
    public void HalfBlockTokenEnumerator_Diagnostic_ShowsAllTokens()
    {
        var topology = new RectangularTopology((3, 4)); // 4 rows = 2 packed rows
        var engine = new HalfBlockLayoutEngine();
        ILayout<Point2D, PackedPoint2D, PackedBounds> layout = engine.CreateLayout(topology);
        var nodeSet = new HashSet<Point2D>(topology.Nodes);

        using var builder = new RectangularGenerationBuilder((3, 4));
        builder[(1, 1)] = true; // Top of packed row 0
        builder[(1, 2)] = true; // Bottom of packed row 1
        using IGeneration<Point2D, bool> generation = builder.Build();

        var theme = new ConsoleTheme(AliveChar: '#', DeadChar: '.', ShowBorder: true);

        var enumerator = new HalfBlockTokenEnumerator(layout, generation, nodeSet, theme);
        int tokenCount = 0;
        int foregroundCount = 0;
        int backgroundCount = 0;
        int backgroundDefaultCount = 0;
        var sb = new StringBuilder();

        while (enumerator.MoveNext())
        {
            Token token = enumerator.Current;
            tokenCount++;
            if (token.IsSequence)
            {
                string seqName = token.Sequence.ToString();
                if (seqName.StartsWith("Foreground", StringComparison.Ordinal))
                {
                    foregroundCount++;
                }
                else if (seqName.StartsWith("Background", StringComparison.Ordinal))
                {
                    backgroundCount++;
                    if (token.Sequence == AnsiSequence.BackgroundDefault)
                    {
                        backgroundDefaultCount++;
                    }
                }

                _ = sb.AppendLine($"Token {tokenCount}: ANSI {token.Sequence}");
            }
            else if (token.Character == '\n')
            {
                _ = sb.AppendLine($"Token {tokenCount}: NEWLINE");
            }
            else
            {
                _ = sb.AppendLine($"Token {tokenCount}: CHAR '{token.Character}'");
            }
        }

        _output.WriteLine(sb.ToString());
        _output.WriteLine($"Total tokens: {tokenCount}");
        _output.WriteLine($"Foreground color tokens: {foregroundCount}");
        _output.WriteLine($"Background color tokens: {backgroundCount}");
        _output.WriteLine($"BackgroundDefault tokens: {backgroundDefaultCount}");

        foregroundCount.ShouldBeGreaterThan(0, "Should have foreground color tokens");
        backgroundCount.ShouldBeGreaterThan(0, "Should have background color tokens");
        backgroundDefaultCount.ShouldBeGreaterThan(0, "Should have BackgroundDefault tokens before borders");
    }
}
