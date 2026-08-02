using FluentValidation;
using Domain.Enums;

namespace Web.Models.Validators;

public class TmaApplicationRequestValidator : AbstractValidator<TmaApplicationRequest>
{
    public TmaApplicationRequestValidator()
    {
        RuleFor(x => x.InitData)
            .NotEmpty().WithMessage("InitData is required.");

        RuleFor(x => x.Country)
            .IsInEnum().WithMessage("Invalid country selected.");

        RuleFor(x => x.CountryOther)
            .NotEmpty().When(x => x.Country == Country.Other)
            .WithMessage("Please specify your country.")
            .MaximumLength(50).WithMessage("Country name is too long.");

        RuleFor(x => x.Profession)
            .NotEmpty().WithMessage("Profession is required.")
            .MaximumLength(100).WithMessage("Profession description is too long.");

        RuleFor(x => x.Term)
            .IsInEnum().WithMessage("Invalid rental term selected.");

        RuleFor(x => x.TermOther)
            .NotEmpty().When(x => x.Term == Term.Other)
            .WithMessage("Please specify the rental term.")
            .MaximumLength(50).WithMessage("Term description is too long.");
    }
}
