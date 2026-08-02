using Bot.Common.Abstractions;
using Bot.Common.Interfaces;
using Bot.Messages.ClientMessages;
using Bot.Session;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Bot.Routers;

public class TextCommandRouter : ITextCommandRouter
{
    private readonly IEnumerable<BaseTextCommand> _baseTextCommands;
    private readonly IBotSessionStore _sessionStore;
    private readonly ClientStartMessage _clientStartMessage;
    private readonly ILogger<TextCommandRouter> _logger;

    public TextCommandRouter(IEnumerable<BaseTextCommand> textCommands,
        IBotSessionStore sessionStore,
        ClientStartMessage clientStartMessage,
        ILogger<TextCommandRouter> logger)
    {
        _baseTextCommands = textCommands;
        _sessionStore = sessionStore;
        _clientStartMessage = clientStartMessage;
        _logger = logger;
    }

    public async Task RouteAsync(ITelegramBotClient client, Update update, long chatId, CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        _logger.LogInformation("Получено сообщение \"{MessageText}\" от пользователя №{ChatId} username {Username}",
            update.Message.Text, chatId, update.Message.Chat.Username);

        var session = await _sessionStore.GetAsync(chatId);

        foreach (var command in _baseTextCommands)
        {
            bool isDirectCommand = command.Name == update.Message.Text;
            bool isStepCommand = command.HandledStep.HasValue && session?.Step == command.HandledStep;

            if (isDirectCommand || isStepCommand)
            {
                await command.Execute(update, client);
                return;
            }
        }

        if (update.Message.ForwardFromChat == null)
        {
            await _clientStartMessage.SendMessage(chatId, client);
        }
    }
}
