using Application.Common.Localization;
using FluentValidation;

namespace Application.Users.Commands.SetUserLanguage;

public class SetUserLanguageRequestValidator : AbstractValidator<SetUserLanguageRequest>
{
    public SetUserLanguageRequestValidator()
    {
        RuleFor(x => x.ChatId)
            .GreaterThan(0).WithMessage("ChatId должен быть больше 0.");

        RuleFor(x => x.Language)
            .Must(SupportedLanguages.IsValid).WithMessage("Указан неподдерживаемый язык.");
    }
}
