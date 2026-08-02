using Bot.Common.Interfaces;
using Bot.Configuration;
using Bot.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot.Common;

public class CommandAnalyzer : ICommandAnalyzer
{
    private readonly IEnumerable<IUpdateHandler> _handlers;
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ILogger<CommandAnalyzer> _logger;
    private readonly AdminNotificationConfiguration _adminNotifications;

    public CommandAnalyzer(IEnumerable<IUpdateHandler> handlers,
        IExceptionNotification exceptionNotification,
        ILogger<CommandAnalyzer> logger,
        IOptions<AdminNotificationConfiguration> adminNotifications)
    {
        _handlers = handlers;
        _exceptionNotification = exceptionNotification;
        _logger = logger;
        _adminNotifications = adminNotifications.Value;
    }

    public async Task AnalyzeCommandsAsync(ITelegramBotClient client, Update update)
    {
        try
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(update.Type));

            if (handler != null)
            {
                await handler.HandleAsync(client, update, default);
            }
            else
            {
                _logger.LogDebug("No handler registered for update type {UpdateType}", update.Type);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AnalyzeCommandsAsync");
            await _exceptionNotification.SendExceptionNotification(client,
                $"[{ex.GetType().Name}] {ex.Message}",
                _adminNotifications.ChatIds.Select(Domain.Common.ChatId.FromLong).ToArray());
        }
    }
}
