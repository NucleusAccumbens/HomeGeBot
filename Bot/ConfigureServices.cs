using Application.Common.Interfaces;
using Bot.Common;
using Bot.Common.Interfaces;
using Bot.Routers;
using Bot.UpdateHandlers;
using Microsoft.Extensions.DependencyInjection;
using Bot.Commands.GeneralCommands.TextCommands;
using Bot.Common.Abstractions;
using Bot.Messages.ClientMessages;
using Bot.Commands.ClientCommands.TextCommands;
using Bot.Messages.GeneralMessages;
using Bot.Services;
using Bot.Session;

namespace Bot;

public static class ConfigureService
{
    public static IServiceCollection AddTelegramBotServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<TelegramBot>();
        services.AddSingleton<ITelegramBotClientProvider, TelegramBot>(s => s.GetRequiredService<TelegramBot>());
        services.AddSingleton<IWebhookSetupService, WebhookSetupService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddSingleton<IBotI18n, BotI18n>();
        services.AddScoped<IBotSessionStore, DistributedBotSessionStore>();
        services.AddScoped<ICommandAnalyzer, CommandAnalyzer>();
        services.AddScoped<IExceptionNotification, ExceptionNotification>();
        services.AddScoped<IUserNotifier, UserNotifier>();
        services.AddScoped<IManagerNotificationFormatter, ManagerNotificationFormatter>();

        // Update handlers
        services.AddScoped<IUpdateHandler, MyChatMemberUpdateHandler>();
        services.AddScoped<IUpdateHandler, MessageUpdateHandler>();
        services.AddScoped<IUpdateHandler, CallbackQueryUpdateHandler>();

        // Routers and helpers
        services.AddScoped<IUserStatusChecker, UserStatusChecker>();
        services.AddScoped<ITextCommandRouter, TextCommandRouter>();
        services.AddScoped<ICallbackCommandRouter, CallbackCommandRouter>();
        services.AddScoped<ILocalizedMessageResolver, LocalizedMessageResolver>();
        services.AddScoped<IRentalApplicationForwardProcessor, RentalApplicationForwardProcessor>();

        AddMessages(services);
        AddTextCommands(services);
        AddCallbackCommands(services);


        return services;
    }

    private static void AddMessages(IServiceCollection services)
    {
        services.AddScoped<ClientStartMessage>();
        services.AddScoped<FlatMessage>();
        services.AddScoped<AdminStartMessage>();
        services.AddScoped<ManagerStartMessage>();
    }

    private static void AddTextCommands(IServiceCollection services)
    {
        services.AddScoped<BaseTextCommand, StartTextCommand>();
        services.AddScoped<BaseTextCommand, AppTextCommand>();
    }

    private static void AddCallbackCommands(IServiceCollection services)
    {
    }
}
