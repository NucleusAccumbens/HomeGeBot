using Application.Common.Results;
using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors;

public class ResultValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, Result<TResult>>
    where TRequest : IRequest<Result<TResult>>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ResultValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Result<TResult>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<TResult>> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));
            return Result<TResult>.Failure($"Ошибка валидации: {errors}");
        }

        return await next();
    }
}
