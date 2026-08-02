using FluentValidation;

namespace Application.AdminManagement.Commands.GrantAdminRights;

public class GrantAdminRightsRequestValidator : AbstractValidator<GrantAdminRightsRequest>
{
    public GrantAdminRightsRequestValidator()
    {
        RuleFor(x => x.SuperAdminChatId)
            .GreaterThan(0).WithMessage("SuperAdminChatId должен быть больше 0.");

        RuleFor(x => x.TargetUserChatId)
            .GreaterThan(0).WithMessage("TargetUserChatId должен быть больше 0.");
    }
}
