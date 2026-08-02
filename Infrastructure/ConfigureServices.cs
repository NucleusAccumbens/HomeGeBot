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
        services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        var connectionStringProvider = new ConnectionStringProvider(configuration);
        var connectionString = connectionStringProvider.GetConnectionString();

        services.AddDbContext<HomeGeBotDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IBotDbContext>(provider => 
            provider.GetRequiredService<HomeGeBotDbContext>());

        services.AddSingleton<IDateTime, DateTimeService>();

        return services;
    }
}
