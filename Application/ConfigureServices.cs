using Application.AdminManagement;
using Application.BotStart;
using Application.Dashboard;
using Application.Messages.Queries;
using Application.RentalApplications;
using Application.TlgUsers.Commands;
using Application.TlgUsers.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Команды, используемые напрямую из Bot-слоя
        services.AddScoped<IKickTlgUserCommand, KickTlgUserCommand>();
        services.AddScoped<IUpdateTlgUserCommand, UpdateTlgUserCommand>();
        services.AddScoped<IGetMessageQuery, GetMessageQuery>();

        // Use Cases
        services.AddScoped<IStartBotUseCase, StartBotUseCase>();
        services.AddScoped<ISubmitRentalApplicationUseCase, SubmitRentalApplicationUseCase>();
        services.AddScoped<IGetBotUsersUseCase, GetBotUsersUseCase>();
        services.AddScoped<IGrantAdminRightsUseCase, GrantAdminRightsUseCase>();
        services.AddScoped<IRevokeAdminRightsUseCase, RevokeAdminRightsUseCase>();
        services.AddScoped<IGetAdminDashboardUseCase, GetAdminDashboardUseCase>();
        services.AddScoped<IUpdateFlatCommentUseCase, UpdateFlatCommentUseCase>();
        services.AddScoped<IDeleteFlatUseCase, DeleteFlatUseCase>();

        return services;
    }
}

