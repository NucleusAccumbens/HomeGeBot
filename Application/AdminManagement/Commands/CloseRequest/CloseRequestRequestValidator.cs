using FluentValidation;

namespace Application.AdminManagement.Commands.CloseRequest;

public class CloseRequestRequestValidator : AbstractValidator<CloseRequestRequest>
{
    public CloseRequestRequestValidator()
    {
        RuleFor(x => x.AdminChatId).GreaterThan(0);
        RuleFor(x => x.ClientChatId).GreaterThan(0);
        RuleFor(x => x.ClientId).GreaterThan(0);
    }
}
