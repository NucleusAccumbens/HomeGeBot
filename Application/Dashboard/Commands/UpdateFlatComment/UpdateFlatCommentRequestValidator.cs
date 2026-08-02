using FluentValidation;

namespace Application.Dashboard.Commands.UpdateFlatComment;

public class UpdateFlatCommentRequestValidator : AbstractValidator<UpdateFlatCommentRequest>
{
    public UpdateFlatCommentRequestValidator()
    {
        RuleFor(x => x.AdminChatId)
            .GreaterThan(0).WithMessage("AdminChatId должен быть больше 0.");
        
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("ItemId обязателен.");
        
        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Комментарий не может превышать 1000 символов.");
    }
}
