using Application.Common.Validation;
using FluentValidation;

namespace Application.AdminManagement.Commands.ReassignClient;

public class ReassignClientRequestValidator : AbstractValidator<ReassignClientRequest>
{
    public ReassignClientRequestValidator()
    {
        RuleFor(x => x.SuperAdminChatId).MustBeValidChatId();
        RuleFor(x => x.ClientChatId).MustBeValidChatId();
        RuleFor(x => x.NewManagerChatId).MustBeValidChatId();
    }
}
