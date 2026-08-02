using FluentValidation;

namespace Application.AdminManagement.Commands.ReassignClient;

public class ReassignClientRequestValidator : AbstractValidator<ReassignClientRequest>
{
    public ReassignClientRequestValidator()
    {
        RuleFor(x => x.SuperAdminChatId)
            .GreaterThan(0).WithMessage("SuperAdminChatId должен быть больше 0.");

        RuleFor(x => x.ClientChatId)
            .GreaterThan(0).WithMessage("ClientChatId должен быть больше 0.");

        RuleFor(x => x.NewManagerChatId)
            .GreaterThan(0).WithMessage("NewManagerChatId должен быть больше 0.");
    }
}
