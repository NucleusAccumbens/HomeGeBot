using Application.Common.Validation;
using FluentValidation;

namespace Application.AdminManagement.Commands.CloseRequest;

public class CloseRequestRequestValidator : AbstractValidator<CloseRequestRequest>
{
    public CloseRequestRequestValidator()
    {
        RuleFor(x => x.AdminChatId).MustBeValidChatId();
        RuleFor(x => x.ClientChatId).MustBeValidChatId();
        RuleFor(x => x.ClientId).GreaterThan(0);
    }
}
