using Application.Common.Behaviors;
using Application.Common.Localization;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMemoryCache();

        // MediatR - все use cases (автоматически регистрирует все Handlers)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Validation behaviors (выполняются первыми)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ResultValidationBehavior<,>));

            // Authorization behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ResultAuthorizationBehavior<,>));

            // SuperAdmin authorization behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ResultSuperAdminAuthorizationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddSingleton<ITmaLabelProvider, TmaLabelProvider>();

        return services;
    }
}

