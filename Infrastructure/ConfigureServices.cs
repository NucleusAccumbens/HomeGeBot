using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureService
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<HomeGeBotDbContext>(options =>
            options.UseNpgsql(ConnectionStringFactory.GetConnectionString(configuration)));

        services.AddScoped<IBotDbContext>(provider => 
            provider.GetRequiredService<HomeGeBotDbContext>());

        services.AddSingleton<IDateTime, DateTimeService>();

        return services;
    }
}
