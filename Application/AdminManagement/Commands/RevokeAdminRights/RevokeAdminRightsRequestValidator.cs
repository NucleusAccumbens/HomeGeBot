using Application.Common.Validation;
using FluentValidation;

namespace Application.AdminManagement.Commands.RevokeAdminRights;

public class RevokeAdminRightsRequestValidator : AbstractValidator<RevokeAdminRightsRequest>
{
    public RevokeAdminRightsRequestValidator()
    {
        RuleFor(x => x.SuperAdminChatId).MustBeValidChatId();
        RuleFor(x => x.TargetAdminChatId).MustBeValidChatId();
    }
}
