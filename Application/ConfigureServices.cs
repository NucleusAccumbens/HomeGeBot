using Application.Admins.Commands;
using Application.Admins.Interfaces;
using Application.Admins.Queries;
using Application.Clients.Commands;
using Application.Clients.Interfaces;
using Application.Clients.Queries;
using Application.Flats.Command;
using Application.Flats.Interfaces;
using Application.Flats.Queries;
using Application.Messages.Queries;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IKickTlgUserCommand, KickTlgUserCommand>();
        services.AddScoped<ICreateTlgUserCommand, CreateTlgUserCommand>();
        services.AddScoped<ICheckUserIsInDbQuery, CheckUserIsInDbQuery>();
        services.AddScoped<IGetMessageQuery, GetMessageQuery>();
        services.AddScoped<IGetAdminsQuery, GetAdminsQuery>();
        services.AddScoped<ICreateAdminCommand, CreateAdminCommand>();
        services.AddScoped<IUpdateTlgUserCommand, UpdateTlgUserCommand>();
        services.AddScoped<ICreateClientCommand, CreateClientCommand>();
        services.AddScoped<IUpdateAdminCommand, UpdateAdminCommand>();
        services.AddScoped<ICheckFlatIsInBdQuery, CheckFlatIsInBdQuery>();
        services.AddScoped<ICreateFlatCommand, CreateFlatCommand>();
        services.AddScoped<IGetClientDependencyQuery, GetClientDependencyQuery>();
        services.AddScoped<IGetFlatsQuery, GetFlatsQuery>();
        services.AddScoped<ICheckUserIsAdminQuery, CheckUserIsAdminQuery>();
        services.AddScoped<IUpdateFlatCommand, UpdateFlatCommand>();
        services.AddScoped<IDeleteFlatCommand, DeleteFlatCommand>();

        return services;
    }
}

