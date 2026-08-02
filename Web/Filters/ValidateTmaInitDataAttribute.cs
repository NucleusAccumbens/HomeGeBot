using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Web.Services;

namespace Web.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidateTmaInitDataAttribute : TypeFilterAttribute
{
    public ValidateTmaInitDataAttribute() : base(typeof(ValidateTmaInitDataFilter))
    {
    }
}

public sealed class ValidateTmaInitDataFilter : IAsyncActionFilter
{
    private readonly ITmaValidationService _tmaValidation;

    public ValidateTmaInitDataFilter(ITmaValidationService tmaValidation)
    {
        _tmaValidation = tmaValidation;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var initData = GetInitData(context);
        if (string.IsNullOrEmpty(initData))
        {
            context.Result = new BadRequestObjectResult("initData is required");
            return;
        }

        if (!_tmaValidation.ValidateInitData(initData))
        {
            context.Result = new UnauthorizedObjectResult("Invalid initData");
            return;
        }

        var userData = _tmaValidation.GetUserData(initData);
        if (userData is null)
        {
            context.Result = new BadRequestObjectResult("Could not extract user data from initData");
            return;
        }

        context.HttpContext.Items["TmaInitData"] = initData;
        context.HttpContext.Items["TmaUserData"] = userData;
        context.HttpContext.Items["TmaUserId"] = userData.Id;

        await next();
    }

    private static string? GetInitData(ActionExecutingContext context)
    {
        foreach (var (_, value) in context.ActionArguments)
        {
            if (value is string direct)
            {
                return direct;
            }

            if (value is null)
            {
                continue;
            }

            var initDataProperty = value.GetType()
                .GetProperties()
                .FirstOrDefault(p =>
                    p.PropertyType == typeof(string) &&
                    string.Equals(p.Name, "InitData", StringComparison.OrdinalIgnoreCase));

            if (initDataProperty is not null)
            {
                return (string?)initDataProperty.GetValue(value);
            }
        }

        return null;
    }
}
