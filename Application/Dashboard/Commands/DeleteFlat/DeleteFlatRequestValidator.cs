using FluentValidation;

namespace Application.Dashboard.Commands.DeleteFlat;

public class DeleteFlatRequestValidator : AbstractValidator<DeleteFlatRequest>
{
    public DeleteFlatRequestValidator()
    {
        RuleFor(x => x.AdminChatId)
            .GreaterThan(0).WithMessage("AdminChatId должен быть больше 0.");
        
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("ItemId обязателен.");
    }
}
