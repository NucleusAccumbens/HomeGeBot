using Application.Common.Validation;
using FluentValidation;

namespace Application.Dashboard.Commands.DeleteFlat;

public class DeleteFlatRequestValidator : AbstractValidator<DeleteFlatRequest>
{
    public DeleteFlatRequestValidator()
    {
        RuleFor(x => x.AdminChatId).MustBeValidChatId();
        
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("ItemId обязателен.");
    }
}
