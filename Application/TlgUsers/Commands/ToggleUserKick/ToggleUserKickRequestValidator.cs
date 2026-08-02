using FluentValidation;

namespace Application.TlgUsers.Commands.ToggleUserKick;

public class ToggleUserKickRequestValidator : AbstractValidator<ToggleUserKickCommand>
{
    public ToggleUserKickRequestValidator()
    {
        RuleFor(x => x.ChatId).GreaterThan(0);
    }
}
