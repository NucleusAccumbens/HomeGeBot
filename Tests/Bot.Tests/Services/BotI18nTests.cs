using Bot.Services;
using FluentAssertions;

namespace Bot.Tests.Services;

public class BotI18nTests
{
    private readonly BotI18n _i18n = new();

    [Theory]
    [InlineData("ru", "btn.openApplication", "Открыть заявку")]
    [InlineData("en", "btn.openApplication", "Open application")]
    [InlineData("ka", "btn.openApplication", "განაცხადის გახსნა")]
    [InlineData("ru", "btn.write", "Написать")]
    [InlineData("en", "btn.write", "Write")]
    [InlineData("ka", "btn.write", "დაწერა")]
    public void T_ReturnsLocalizedText(string language, string key, string expected)
    {
        _i18n.T(key, language).Should().Be(expected);
    }

    [Fact]
    public void T_UnknownLanguage_FallsBackToRussian()
    {
        _i18n.T("btn.openApplication", "fr").Should().Be("Открыть заявку");
    }

    [Fact]
    public void T_UnknownKey_ReturnsKeyItself()
    {
        _i18n.T("btn.unknown", "ru").Should().Be("btn.unknown");
    }

    [Fact]
    public void T_DefaultLanguageIsRussian()
    {
        _i18n.T("btn.openApplication").Should().Be("Открыть заявку");
    }
}
