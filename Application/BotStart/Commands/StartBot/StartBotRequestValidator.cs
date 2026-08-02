using FluentValidation;

namespace Application.BotStart.Commands.StartBot;

public class StartBotRequestValidator : AbstractValidator<StartBotRequest>
{
    public StartBotRequestValidator()
    {
        RuleFor(x => x.ChatId)
            .GreaterThan(0).WithMessage("ChatId должен быть больше 0.");
    }
}
