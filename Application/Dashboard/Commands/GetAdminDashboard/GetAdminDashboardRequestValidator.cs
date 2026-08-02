using FluentValidation;

namespace Application.Dashboard.Commands.GetAdminDashboard;

public class GetAdminDashboardRequestValidator : AbstractValidator<GetAdminDashboardRequest>
{
    public GetAdminDashboardRequestValidator()
    {
        RuleFor(x => x.AdminChatId)
            .GreaterThan(0).WithMessage("AdminChatId должен быть больше 0.");
    }
}
