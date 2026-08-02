using Application.Common.Validation;
using FluentValidation;

namespace Application.Dashboard.Commands.GetAdminDashboard;

public class GetAdminDashboardRequestValidator : AbstractValidator<GetAdminDashboardRequest>
{
    public GetAdminDashboardRequestValidator()
    {
        RuleFor(x => x.AdminChatId).MustBeValidChatId();
    }
}
