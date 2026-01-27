using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class GlyphTests
{
    [Fact]
    public void Constructor_WithColorAndCharacter_SetsProperties()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        glyph.Color.ShouldBe(AnsiSequence.ForegroundGreen);
        glyph.Character.ShouldBe('A');
        glyph.IsNewline.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_WithNullColor_SetsNullColor()
    {
        var glyph = new Glyph(null, 'B');

        glyph.Color.ShouldBeNull();
        glyph.Character.ShouldBe('B');
    }

    [Fact]
    public void IsNewline_WithNewlineCharacter_ReturnsTrue()
    {
        var glyph = new Glyph(null, '\n');

        glyph.IsNewline.ShouldBeTrue();
    }

    [Fact]
    public void IsNewline_WithOtherCharacter_ReturnsFalse()
    {
        var glyph = new Glyph(null, 'X');

        glyph.IsNewline.ShouldBeFalse();
    }

    [Fact]
    public void Equals_SameGlyphs_ReturnsTrue()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        glyph2.ShouldBe(glyph1);
        (glyph1 == glyph2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentColors_ReturnsFalse()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGray, 'A');

        glyph2.ShouldNotBe(glyph1);
    }

    [Fact]
    public void Equals_DifferentCharacters_ReturnsFalse()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'B');

        glyph2.ShouldNotBe(glyph1);
    }

    [Fact]
    public void Equals_NullColorVsWithColor_ReturnsFalse()
    {
        var glyph1 = new Glyph(null, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        glyph2.ShouldNotBe(glyph1);
    }

    [Fact]
    public void ToString_WithColor_ReturnsFormattedString()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'X');

        string result = glyph.ToString();

        result.ShouldContain("Glyph");
        result.ShouldContain("ForegroundGreen");
        result.ShouldContain("X");
    }

    [Fact]
    public void ToString_WithNewline_ReturnsNewlineFormat()
    {
        var glyph = new Glyph(null, '\n');

        string result = glyph.ToString();

        result.ShouldContain("\\n");
    }

    [Fact]
    public void GetHashCode_SameGlyphs_ReturnsSameHash()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        glyph2.GetHashCode().ShouldBe(glyph1.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentGlyphs_ReturnsDifferentHash()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGray, 'B');

        // Hash codes can theoretically collide, but different values should usually differ
        glyph2.GetHashCode().ShouldNotBe(glyph1.GetHashCode());
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        glyph.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'A');

        glyph.Equals("not a glyph").ShouldBeFalse();
        glyph.Equals(42).ShouldBeFalse();
    }

    [Fact]
    public void NotEquals_DifferentColors_ReturnsTrue()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGray, 'A');

        (glyph1 != glyph2).ShouldBeTrue();
    }

    [Fact]
    public void NotEquals_DifferentCharacters_ReturnsTrue()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, 'B');

        (glyph1 != glyph2).ShouldBeTrue();
    }

    [Fact]
    public void ToString_WithNullColor_ReturnsNullInOutput()
    {
        var glyph = new Glyph(null, 'X');

        string result = glyph.ToString();

        result.ShouldContain("null");
        result.ShouldContain("X");
    }

    [Fact]
    public void Default_HasNullColorAndNullCharacter()
    {
        var glyph = default(Glyph);

        glyph.Color.ShouldBeNull();
        glyph.Character.ShouldBe('\0');
    }

    [Fact]
    public void Equals_BothNullColors_ReturnsTrue()
    {
        var glyph1 = new Glyph(null, 'A');
        var glyph2 = new Glyph(null, 'A');

        glyph1.Equals(glyph2).ShouldBeTrue();
        (glyph1 == glyph2).ShouldBeTrue();
    }

    // Background color tests

    [Fact]
    public void Constructor_WithForegroundBackgroundAndCharacter_SetsAllProperties()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'X');

        glyph.Color.ShouldBe(AnsiSequence.ForegroundGreen);
        glyph.BackgroundColor.ShouldBe(AnsiSequence.BackgroundDarkGray);
        glyph.Character.ShouldBe('X');
    }

    [Fact]
    public void Constructor_TwoArg_SetsBackgroundToNull()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, 'X');

        glyph.BackgroundColor.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullBackground_SetsNullBackground()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, null, 'X');

        glyph.Color.ShouldBe(AnsiSequence.ForegroundGreen);
        glyph.BackgroundColor.ShouldBeNull();
    }

    [Fact]
    public void Equals_SameBackgroundColors_ReturnsTrue()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'A');

        glyph1.ShouldBe(glyph2);
        (glyph1 == glyph2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentBackgroundColors_ReturnsFalse()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDefault, 'A');

        glyph1.ShouldNotBe(glyph2);
        (glyph1 != glyph2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_NullVsNonNullBackground_ReturnsFalse()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, null, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'A');

        glyph1.ShouldNotBe(glyph2);
    }

    [Fact]
    public void GetHashCode_SameBackgrounds_ReturnsSameHash()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'A');

        glyph1.GetHashCode().ShouldBe(glyph2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentBackgrounds_ReturnsDifferentHash()
    {
        var glyph1 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'A');
        var glyph2 = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDefault, 'A');

        glyph1.GetHashCode().ShouldNotBe(glyph2.GetHashCode());
    }

    [Fact]
    public void ToString_WithBackground_ContainsBackgroundInfo()
    {
        var glyph = new Glyph(AnsiSequence.ForegroundGreen, AnsiSequence.BackgroundDarkGray, 'X');

        string result = glyph.ToString();

        result.ShouldContain("BackgroundDarkGray");
    }

    [Fact]
    public void Default_HasNullBackgroundColor()
    {
        var glyph = default(Glyph);

        glyph.BackgroundColor.ShouldBeNull();
    }
}
