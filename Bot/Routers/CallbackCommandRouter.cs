using Bot.Common.Abstractions;
using Bot.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bot.Routers;

public class CallbackCommandRouter : ICallbackCommandRouter
{
    private readonly IEnumerable<BaseCallbackCommand> _baseCallbackCommands;
    private readonly ILogger<CallbackCommandRouter> _logger;

    public CallbackCommandRouter(IEnumerable<BaseCallbackCommand> callbackCommands,
        ILogger<CallbackCommandRouter> logger)
    {
        _baseCallbackCommands = callbackCommands;
        _logger = logger;
    }

    public async Task RouteAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery?.Data == null || update.CallbackQuery.Message == null) return;

        _logger.LogInformation("Получена команда \"{CallbackData}\" от пользователя №{ChatId} username {Username}",
            update.CallbackQuery.Data, update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Chat.Username);

        foreach (var command in _baseCallbackCommands)
        {
            if (command.Contains(update.CallbackQuery))
            {
                await command.CallbackExecute(update, client);
            }
        }
    }
}
