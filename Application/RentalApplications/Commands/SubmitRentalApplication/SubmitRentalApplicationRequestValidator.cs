using Application.Common.Validation;
using FluentValidation;

namespace Application.RentalApplications.Commands.SubmitRentalApplication;

public class SubmitRentalApplicationRequestValidator : AbstractValidator<SubmitRentalApplicationRequest>
{
    public SubmitRentalApplicationRequestValidator()
    {
        RuleFor(x => x.ChatId).MustBeValidChatId();

        RuleFor(x => x.Profession)
            .NotEmpty().WithMessage("Профессия обязательна.")
            .MaximumLength(100).WithMessage("Профессия не может превышать 100 символов.");
    }
}
