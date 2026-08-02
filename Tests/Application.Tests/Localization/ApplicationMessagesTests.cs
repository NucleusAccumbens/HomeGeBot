using Application.Common.Localization;
using FluentAssertions;

namespace Application.Tests.Localization;

public class ApplicationMessagesTests
{
    [Theory]
    [InlineData("ru", "Вы уже отправили 5 заявок — это максимальное количество.")]
    [InlineData("en", "You have already submitted 5 applications — this is the maximum.")]
    [InlineData("ka", "თქვენ უკვე გაგზავნეთ 5 განაცხადი — ეს მაქსიმალური რაოდენობაა.")]
    [InlineData("fr", "Вы уже отправили 5 заявок — это максимальное количество.")]
    [InlineData("", "Вы уже отправили 5 заявок — это максимальное количество.")]
    public void ApplicationLimitExceeded_ReturnsLocalizedText(string language, string expected)
    {
        ApplicationMessages.ApplicationLimitExceeded(language).Should().Be(expected);
    }

    [Theory]
    [InlineData("ru", "Нет активных менеджеров")]
    [InlineData("en", "No active managers available.")]
    [InlineData("ka", "აქტიური მენეჯერები არ არის.")]
    [InlineData("fr", "Нет активных менеджеров")]
    public void NoActiveManagers_ReturnsLocalizedText(string language, string expected)
    {
        ApplicationMessages.NoActiveManagers(language).Should().Be(expected);
    }
}
