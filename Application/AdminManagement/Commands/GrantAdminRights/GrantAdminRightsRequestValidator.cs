using Application.Common.Validation;
using FluentValidation;

namespace Application.AdminManagement.Commands.GrantAdminRights;

public class GrantAdminRightsRequestValidator : AbstractValidator<GrantAdminRightsRequest>
{
    public GrantAdminRightsRequestValidator()
    {
        RuleFor(x => x.SuperAdminChatId).MustBeValidChatId();
        RuleFor(x => x.TargetUserChatId).MustBeValidChatId();
    }
}
