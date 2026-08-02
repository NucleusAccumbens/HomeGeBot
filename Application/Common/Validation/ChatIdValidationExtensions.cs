using Domain.Common;
using FluentValidation;

namespace Application.Common.Validation;

public static class ChatIdValidationExtensions
{
    public static IRuleBuilderOptions<T, ChatId> MustBeValidChatId<T>(this IRuleBuilder<T, ChatId> ruleBuilder)
    {
        return ruleBuilder.Must(c => c.Value > 0).WithMessage("ChatId должен быть больше 0.");
    }
}
