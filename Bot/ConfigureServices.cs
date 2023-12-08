using Bot.Common.Services;
using Bot.Common;
using Microsoft.Extensions.DependencyInjection;
using Bot.Commands.GeneralCommands.TextCommands;
using Bot.Common.Abstractions;
using Bot.Commands.ClientCommands.CallbackCommands;
using Bot.Messages.ClientMessages;
using Bot.Commands.ClientCommands.TextCommands;
using Bot.Messages.GeneralMessages;
using Bot.Services;

namespace Bot;

public static class ConfigureService
{
    public static IServiceCollection AddTelegramBotServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<TelegramBot>();
        services.AddScoped<IMemoryCacheService, MemoryCachService>();
        services.AddScoped<ICommandAnalyzer, CommandAnalyzer>();
        services.AddSingleton<IExceptionNotification, ExceptionNotification>();

        AddMessages(services);
        AddTextCommands(services);
        AddCallbackCommands(services);


        return services;
    }

    private static void AddMessages(IServiceCollection services)
    {
        services.AddScoped<CountryMessage>();
        services.AddScoped<ProfessionMessage>();
        services.AddScoped<HasPetsMessage>();
        services.AddScoped<TermMessage>();
        services.AddScoped<FlatMessage>();
        services.AddScoped<AdminStartMessage>();
    }

    private static void AddTextCommands(IServiceCollection services)
    {
        services.AddScoped<BaseTextCommand, StartTextCommand>();
        services.AddScoped<BaseTextCommand, ProfessionTextCommand>();
        services.AddScoped<BaseTextCommand, AddAdminTextCommand>();
        services.AddScoped<BaseTextCommand, AppTextCommand>();
    }

    private static void AddCallbackCommands(IServiceCollection services)
    {
        services.AddScoped<BaseCallbackCommand, CountryCallbackCommand>();
        services.AddScoped<BaseCallbackCommand, HasPetsCallbackCommand>();
        services.AddScoped<BaseCallbackCommand, TermCallbackCommand>();
        services.AddScoped<BaseCallbackCommand, FlatCallbackCommand>();
    }
}
