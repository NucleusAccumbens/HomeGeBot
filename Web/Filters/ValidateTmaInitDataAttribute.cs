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
    private readonly ITmaInitDataParser _parser;
    private readonly ITmaInitDataValidator _validator;

    public ValidateTmaInitDataFilter(ITmaInitDataParser parser, ITmaInitDataValidator validator)
    {
        _parser = parser;
        _validator = validator;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var initData = GetInitData(context);
        if (string.IsNullOrEmpty(initData))
        {
            context.Result = new BadRequestObjectResult("initData is required");
            return;
        }

        var data = _parser.Parse(initData);

        if (!_validator.Validate(data))
        {
            context.Result = new UnauthorizedObjectResult("Invalid initData");
            return;
        }

        var userData = _parser.GetUserData(data);
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
