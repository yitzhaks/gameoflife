using Xunit;

namespace GameOfLife.Rendering.Console.Tests;

public class TokenTests
{
    [Fact]
    public void Char_CreatesCharacterToken()
    {
        var token = Token.Char('A');

        Assert.True(token.IsCharacter);
        Assert.False(token.IsSequence);
        Assert.Equal('A', token.Character);
    }

    [Fact]
    public void Ansi_CreatesSequenceToken()
    {
        var token = Token.Ansi(AnsiSequence.ForegroundGreen);

        Assert.True(token.IsSequence);
        Assert.False(token.IsCharacter);
        Assert.Equal(AnsiSequence.ForegroundGreen, token.Sequence);
    }

    [Fact]
    public void Equals_SameCharacterTokens_ReturnsTrue()
    {
        var token1 = Token.Char('X');
        var token2 = Token.Char('X');

        Assert.Equal(token1, token2);
        Assert.True(token1 == token2);
    }

    [Fact]
    public void Equals_DifferentCharacterTokens_ReturnsFalse()
    {
        var token1 = Token.Char('X');
        var token2 = Token.Char('Y');

        Assert.NotEqual(token1, token2);
        Assert.True(token1 != token2);
    }

    [Fact]
    public void Equals_SameSequenceTokens_ReturnsTrue()
    {
        var token1 = Token.Ansi(AnsiSequence.ForegroundGreen);
        var token2 = Token.Ansi(AnsiSequence.ForegroundGreen);

        Assert.Equal(token1, token2);
    }

    [Fact]
    public void Equals_DifferentSequenceTokens_ReturnsFalse()
    {
        var token1 = Token.Ansi(AnsiSequence.ForegroundGreen);
        var token2 = Token.Ansi(AnsiSequence.ForegroundGray);

        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void Equals_CharacterAndSequenceToken_ReturnsFalse()
    {
        var charToken = Token.Char('A');
        var seqToken = Token.Ansi(AnsiSequence.Reset);

        Assert.NotEqual(charToken, seqToken);
    }

    [Fact]
    public void ToString_CharacterToken_ReturnsFormattedString()
    {
        var token = Token.Char('Z');

        Assert.Contains("Char", token.ToString());
        Assert.Contains("Z", token.ToString());
    }

    [Fact]
    public void ToString_SequenceToken_ReturnsFormattedString()
    {
        var token = Token.Ansi(AnsiSequence.ForegroundGreen);

        Assert.Contains("Ansi", token.ToString());
        Assert.Contains("ForegroundGreen", token.ToString());
    }
}

public class WellKnownTokensTests
{
    [Fact]
    public void Newline_IsNewlineCharacter()
    {
        Assert.True(WellKnownTokens.Newline.IsCharacter);
        Assert.Equal('\n', WellKnownTokens.Newline.Character);
    }

    [Fact]
    public void Space_IsSpaceCharacter()
    {
        Assert.True(WellKnownTokens.Space.IsCharacter);
        Assert.Equal(' ', WellKnownTokens.Space.Character);
    }

    [Fact]
    public void Green_IsGreenSequence()
    {
        Assert.True(WellKnownTokens.Green.IsSequence);
        Assert.Equal(AnsiSequence.ForegroundGreen, WellKnownTokens.Green.Sequence);
    }

    [Fact]
    public void Reset_IsResetSequence()
    {
        Assert.True(WellKnownTokens.Reset.IsSequence);
        Assert.Equal(AnsiSequence.Reset, WellKnownTokens.Reset.Sequence);
    }
}
