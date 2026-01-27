using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class GlyphTests
{
    [Fact]
    public void Constructor_WithColorAndCharacter_SetsProperties()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        Assert.Equal(AnsiSequence.ForegroundGreen, glyph.Color);
        Assert.Equal('A', glyph.Character);
        Assert.False(glyph.IsNewline);
    }

    [Fact]
    public void Constructor_WithNullColor_SetsNullColor()
    {
        var glyph = new Glyph(null, 'B');

        Assert.Null(glyph.Color);
        Assert.Equal('B', glyph.Character);
    }

    [Fact]
    public void IsNewline_WithNewlineCharacter_ReturnsTrue()
    {
        var glyph = new Glyph(null, '\n');

        Assert.True(glyph.IsNewline);
    }

    [Fact]
    public void IsNewline_WithOtherCharacter_ReturnsFalse()
    {
        var glyph = new Glyph(null, 'X');

        Assert.False(glyph.IsNewline);
    }

    [Fact]
    public void Equals_SameGlyphs_ReturnsTrue()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        Assert.Equal(glyph1, glyph2);
        Assert.True(glyph1 == glyph2);
    }

    [Fact]
    public void Equals_DifferentColors_ReturnsFalse()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGray, 'A');

        Assert.NotEqual(glyph1, glyph2);
    }

    [Fact]
    public void Equals_DifferentCharacters_ReturnsFalse()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'B');

        Assert.NotEqual(glyph1, glyph2);
    }

    [Fact]
    public void Equals_NullColorVsWithColor_ReturnsFalse()
    {
        var glyph1 = new Glyph(null, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        Assert.NotEqual(glyph1, glyph2);
    }

    [Fact]
    public void ToString_WithColor_ReturnsFormattedString()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'X');

        string result = glyph.ToString();

        Assert.Contains("Glyph", result);
        Assert.Contains("ForegroundGreen", result);
        Assert.Contains("X", result);
    }

    [Fact]
    public void ToString_WithNewline_ReturnsNewlineFormat()
    {
        var glyph = new Glyph(null, '\n');

        string result = glyph.ToString();

        Assert.Contains("\\n", result);
    }

    [Fact]
    public void GetHashCode_SameGlyphs_ReturnsSameHash()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        Assert.Equal(glyph1.GetHashCode(), glyph2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentGlyphs_ReturnsDifferentHash()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGray, 'B');

        // Hash codes can theoretically collide, but different values should usually differ
        Assert.NotEqual(glyph1.GetHashCode(), glyph2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        Assert.False(glyph.Equals(null));
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        Assert.False(glyph.Equals("not a glyph"));
        Assert.False(glyph.Equals(42));
    }

    [Fact]
    public void NotEquals_DifferentColors_ReturnsTrue()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGray, 'A');

        Assert.True(glyph1 != glyph2);
    }

    [Fact]
    public void NotEquals_DifferentCharacters_ReturnsTrue()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'B');

        Assert.True(glyph1 != glyph2);
    }

    [Fact]
    public void ToString_WithNullColor_ReturnsNullInOutput()
    {
        var glyph = new Glyph(null, 'X');

        string result = glyph.ToString();

        Assert.Contains("null", result);
        Assert.Contains("X", result);
    }

    [Fact]
    public void Default_HasNullColorAndNullCharacter()
    {
        var glyph = default(Glyph);

        Assert.Null(glyph.Color);
        Assert.Equal('\0', glyph.Character);
    }

    [Fact]
    public void Equals_BothNullColors_ReturnsTrue()
    {
        var glyph1 = new Glyph(null, 'A');
        var glyph2 = new Glyph(null, 'A');

        Assert.True(glyph1.Equals(glyph2));
        Assert.True(glyph1 == glyph2);
    }
}
