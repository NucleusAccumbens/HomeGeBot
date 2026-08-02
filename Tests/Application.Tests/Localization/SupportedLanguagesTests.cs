using Application.Common.Localization;
using FluentAssertions;

namespace Application.Tests.Localization;

public class SupportedLanguagesTests
{
    [Fact]
    public void Default_IsRussian()
    {
        SupportedLanguages.Default.Should().Be("ru");
    }

    [Fact]
    public void All_ContainsThreeLanguages()
    {
        SupportedLanguages.All.Should().BeEquivalentTo(new[] { "ru", "en", "ka" });
    }

    [Theory]
    [InlineData("ru", true)]
    [InlineData("en", true)]
    [InlineData("ka", true)]
    [InlineData("fr", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValid_DetectsSupportedLanguages(string? language, bool expected)
    {
        SupportedLanguages.IsValid(language).Should().Be(expected);
    }
}
