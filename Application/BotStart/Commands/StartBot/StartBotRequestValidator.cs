using Application.Common.Validation;
using FluentValidation;

namespace Application.BotStart.Commands.StartBot;

public class StartBotRequestValidator : AbstractValidator<StartBotRequest>
{
    public StartBotRequestValidator()
    {
        RuleFor(x => x.ChatId).MustBeValidChatId();
    }
}
