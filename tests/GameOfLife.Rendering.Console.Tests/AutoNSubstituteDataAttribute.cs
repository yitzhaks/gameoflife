using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace GameOfLife.Rendering.Console.Tests;

public sealed class AutoNSubstituteDataAttribute : AutoDataAttribute
{
    public AutoNSubstituteDataAttribute()
        : base(() => new Fixture()
            .Customize(new AutoNSubstituteCustomization())
            .Customize(new GlyphCustomization()))
    { }

    private sealed class GlyphCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new Glyph(
                fixture.Create<AnsiSequence?>(),
                fixture.Create<char>()));
        }
    }
}
