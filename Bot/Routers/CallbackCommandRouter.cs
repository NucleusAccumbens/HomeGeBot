using Bot.Common.Abstractions;
using Bot.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bot.Routers;

public class CallbackCommandRouter : ICallbackCommandRouter
{
    private readonly IReadOnlyDictionary<char, BaseCallbackCommand> _commandsByCode;
    private readonly ILogger<CallbackCommandRouter> _logger;

    public CallbackCommandRouter(IEnumerable<BaseCallbackCommand> callbackCommands,
        ILogger<CallbackCommandRouter> logger)
    {
        var commands = new Dictionary<char, BaseCallbackCommand>();

        foreach (var command in callbackCommands)
        {
            commands[command.CallbackDataCode] = command;
        }

        _commandsByCode = commands;
        _logger = logger;
    }

    public async Task RouteAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery?.Data == null || update.CallbackQuery.Message == null) return;

        _logger.LogInformation("Получена команда \"{CallbackData}\" от пользователя №{ChatId} username {Username}",
            update.CallbackQuery.Data, update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Chat.Username);

        var code = update.CallbackQuery.Data.FirstOrDefault();

        if (_commandsByCode.TryGetValue(code, out var command))
        {
            await command.CallbackExecute(update, client);
            return;
        }

        _logger.LogDebug("No callback command registered for code {CallbackDataCode}", code);
    }
}
