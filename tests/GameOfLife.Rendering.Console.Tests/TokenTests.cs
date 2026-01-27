using Shouldly;

using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class TokenTests
{
    [Fact]
    public void Char_CreatesCharacterToken()
    {
        var token = Token.Char('A');

        token.IsCharacter.ShouldBeTrue();
        token.IsSequence.ShouldBeFalse();
        token.Character.ShouldBe('A');
    }

    [Fact]
    public void Ansi_CreatesSequenceToken()
    {
        var token = Token.Ansi(AnsiSequence.ForegroundGreen);

        token.IsSequence.ShouldBeTrue();
        token.IsCharacter.ShouldBeFalse();
        token.Sequence.ShouldBe(AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void Equals_SameCharacterTokens_ReturnsTrue()
    {
        var token1 = Token.Char('X');
        var token2 = Token.Char('X');

        token1.ShouldBe(token2);
        (token1 == token2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentCharacterTokens_ReturnsFalse()
    {
        var token1 = Token.Char('X');
        var token2 = Token.Char('Y');

        token1.ShouldNotBe(token2);
        (token1 != token2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_SameSequenceTokens_ReturnsTrue()
    {
        var token1 = Token.Ansi(AnsiSequence.ForegroundGreen);
        var token2 = Token.Ansi(AnsiSequence.ForegroundGreen);

        token1.ShouldBe(token2);
    }

    [Fact]
    public void Equals_DifferentSequenceTokens_ReturnsFalse()
    {
        var token1 = Token.Ansi(AnsiSequence.ForegroundGreen);
        var token2 = Token.Ansi(AnsiSequence.ForegroundGray);

        token1.ShouldNotBe(token2);
    }

    [Fact]
    public void Equals_CharacterAndSequenceToken_ReturnsFalse()
    {
        var charToken = Token.Char('A');
        var seqToken = Token.Ansi(AnsiSequence.Reset);

        charToken.ShouldNotBe(seqToken);
    }

    [Fact]
    public void ToString_CharacterToken_ReturnsFormattedString()
    {
        var token = Token.Char('Z');

        token.ToString().ShouldContain("Char");
        token.ToString().ShouldContain("Z");
    }

    [Fact]
    public void ToString_SequenceToken_ReturnsFormattedString()
    {
        var token = Token.Ansi(AnsiSequence.ForegroundGreen);

        token.ToString().ShouldContain("Ansi");
        token.ToString().ShouldContain("ForegroundGreen");
    }
}

public class WellKnownTokensTests
{
    [Fact]
    public void Newline_IsNewlineCharacter()
    {
        WellKnownTokens.Newline.IsCharacter.ShouldBeTrue();
        WellKnownTokens.Newline.Character.ShouldBe('\n');
    }

    [Fact]
    public void Space_IsSpaceCharacter()
    {
        WellKnownTokens.Space.IsCharacter.ShouldBeTrue();
        WellKnownTokens.Space.Character.ShouldBe(' ');
    }

    [Fact]
    public void Green_IsGreenSequence()
    {
        WellKnownTokens.Green.IsSequence.ShouldBeTrue();
        WellKnownTokens.Green.Sequence.ShouldBe(AnsiSequence.ForegroundGreen);
    }

    [Fact]
    public void Reset_IsResetSequence()
    {
        WellKnownTokens.Reset.IsSequence.ShouldBeTrue();
        WellKnownTokens.Reset.Sequence.ShouldBe(AnsiSequence.Reset);
    }

    [Fact]
    public void DarkGray_IsDarkGraySequence()
    {
        WellKnownTokens.DarkGray.IsSequence.ShouldBeTrue();
        WellKnownTokens.DarkGray.Sequence.ShouldBe(AnsiSequence.ForegroundDarkGray);
    }

    [Fact]
    public void Gray_IsGraySequence()
    {
        WellKnownTokens.Gray.IsSequence.ShouldBeTrue();
        WellKnownTokens.Gray.Sequence.ShouldBe(AnsiSequence.ForegroundGray);
    }
}

public class TokenGetHashCodeTests
{
    [Fact]
    public void GetHashCode_SameCharTokens_ReturnsSameHash()
    {
        var token1 = Token.Char('X');
        var token2 = Token.Char('X');

        token1.GetHashCode().ShouldBe(token2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_SameSequenceTokens_ReturnsSameHash()
    {
        var token1 = Token.Ansi(AnsiSequence.ForegroundGreen);
        var token2 = Token.Ansi(AnsiSequence.ForegroundGreen);

        token1.GetHashCode().ShouldBe(token2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var token = Token.Char('X');

        token.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        var token = Token.Char('X');

        token.Equals("not a token").ShouldBeFalse();
        token.Equals(42).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_DifferentCharTokens_ReturnsDifferentHash()
    {
        var token1 = Token.Char('X');
        var token2 = Token.Char('Y');

        token1.GetHashCode().ShouldNotBe(token2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_CharAndSequenceTokens_ReturnsDifferentHash()
    {
        var charToken = Token.Char('X');
        var seqToken = Token.Ansi(AnsiSequence.ForegroundGreen);

        charToken.GetHashCode().ShouldNotBe(seqToken.GetHashCode());
    }

    [Fact]
    public void NotEquals_CharTokens_ReturnsTrue()
    {
        var token1 = Token.Char('X');
        var token2 = Token.Char('Y');

        (token1 != token2).ShouldBeTrue();
    }

    [Fact]
    public void NotEquals_SequenceTokens_ReturnsTrue()
    {
        var token1 = Token.Ansi(AnsiSequence.ForegroundGreen);
        var token2 = Token.Ansi(AnsiSequence.ForegroundGray);

        (token1 != token2).ShouldBeTrue();
    }

    [Fact]
    public void Sequence_OnCharToken_ReturnsDefault()
    {
        var token = Token.Char('X');

        // Accessing Sequence on a char token should return default (0)
        token.Sequence.ShouldBe(AnsiSequence.Reset); // Reset is 0, the default
    }

    [Fact]
    public void Character_OnSequenceToken_ReturnsDefault()
    {
        var token = Token.Ansi(AnsiSequence.ForegroundGreen);

        // Accessing Character on a sequence token should return default ('\0')
        token.Character.ShouldBe('\0');
    }
}
