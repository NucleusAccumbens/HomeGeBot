using Application.Common.Localization;
using Application.Common.Validation;
using FluentValidation;

namespace Application.Users.Commands.SetUserLanguage;

public class SetUserLanguageRequestValidator : AbstractValidator<SetUserLanguageRequest>
{
    public SetUserLanguageRequestValidator()
    {
        RuleFor(x => x.ChatId).MustBeValidChatId();

        RuleFor(x => x.Language)
            .Must(SupportedLanguages.IsValid).WithMessage("Указан неподдерживаемый язык.");
    }
}
