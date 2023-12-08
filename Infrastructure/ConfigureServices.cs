using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureService
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        if (configuration.GetValue<bool>("InDeveloping"))
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");


            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"THE Add Infrastructure Services METHOD WORKED. CONNECTION STRING: " +
                $"{connectionString}");
            Console.ResetColor();
            
            services.AddDbContext<ThisBotDbContext>(optionBuilder =>
            optionBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"THE Add Infrastructure Services METHOD WORKED. CONNECTION STRING: {GetConnectionString(configuration)}");
            Console.ResetColor();

            services.AddDbContext<ThisBotDbContext>(options =>
            options.UseNpgsql(GetConnectionString(configuration)));           
        }

        services.AddScoped<IBotDbContext>(provider => 
        provider.GetRequiredService<ThisBotDbContext>());

        services.AddTransient<IDateTime, DateTimeService>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        string? connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (connectionUrl != null)
        {
            string userPassSide = connectionUrl.Split("@")[0];
            string hostSide = connectionUrl.Split("@")[1];
            string user = userPassSide.Split(":")[1][2..];
            string password = userPassSide.Split(':')[2];
            string host = hostSide.Split("/")[0];
            var database = hostSide.Split("/")[1].Split("?")[0];

            return $"Host={host};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }

        return $"{configuration.GetConnectionString("DefaultConnection")}";
    }
}
