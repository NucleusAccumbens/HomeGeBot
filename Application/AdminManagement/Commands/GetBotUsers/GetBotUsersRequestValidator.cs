using FluentValidation;

namespace Application.AdminManagement.Commands.GetBotUsers;

public class GetBotUsersRequestValidator : AbstractValidator<GetBotUsersRequest>
{
    public GetBotUsersRequestValidator()
    {
        RuleFor(x => x.SuperAdminChatId)
            .GreaterThan(0).WithMessage("SuperAdminChatId должен быть больше 0.");
    }
}
