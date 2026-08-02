using FluentValidation;

namespace Application.AdminManagement.Commands.RevokeAdminRights;

public class RevokeAdminRightsRequestValidator : AbstractValidator<RevokeAdminRightsRequest>
{
    public RevokeAdminRightsRequestValidator()
    {
        RuleFor(x => x.SuperAdminChatId)
            .GreaterThan(0).WithMessage("SuperAdminChatId должен быть больше 0.");

        RuleFor(x => x.TargetAdminChatId)
            .GreaterThan(0).WithMessage("TargetAdminChatId должен быть больше 0.");
    }
}
