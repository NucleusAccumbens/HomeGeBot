using Application.Common.Validation;
using FluentValidation;

namespace Application.AdminManagement.Commands.GetBotUsers;

public class GetBotUsersRequestValidator : AbstractValidator<GetBotUsersRequest>
{
    public GetBotUsersRequestValidator()
    {
        RuleFor(x => x.SuperAdminChatId).MustBeValidChatId();
    }
}
