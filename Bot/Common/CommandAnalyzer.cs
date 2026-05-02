using Application.TlgUsers.Interfaces;
using Bot.Common.Abstractions;
using Bot.Configuration;
using Bot.Session;
using Logger.Interfaces;
using Microsoft.Extensions.Options;

namespace Bot.Common;

public class CommandAnalyzer : ICommandAnalyzer
{
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ICustomLogger _logger;
    private readonly IEnumerable<BaseTextCommand> _baseTextCommands;
    private readonly IEnumerable<BaseCallbackCommand> _baseCallbackCommands;
    private readonly IBotSessionStore _sessionStore;
    private readonly IKickTlgUserCommand _kickTlgUserCommand;
    private readonly AdminNotificationConfiguration _adminNotifications;

    public CommandAnalyzer(IEnumerable<BaseTextCommand> textCommands,
        IEnumerable<BaseCallbackCommand> callbackCommands,
        IBotSessionStore sessionStore,
        IKickTlgUserCommand kickTlgUserCommand, IExceptionNotification exceptionNotification,
        ICustomLogger logger, IOptions<AdminNotificationConfiguration> adminNotifications)
    {
        _baseTextCommands = textCommands;
        _baseCallbackCommands = callbackCommands;
        _sessionStore = sessionStore;
        _kickTlgUserCommand = kickTlgUserCommand;
        _exceptionNotification = exceptionNotification;
        _logger = logger;
        _adminNotifications = adminNotifications.Value;
    }

    public async Task AnalyzeCommandsAsync(ITelegramBotClient client, Update update)
    {
        try
        {
            if (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
            {
                await _kickTlgUserCommand.ManageTlgUserKickingAsync(update.MyChatMember.Chat.Id);
            }
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                bool isKicked = await _kickTlgUserCommand
                        .CheckTlgUserIsKicked(update.CallbackQuery.Message?.Chat.Id);

                if (!isKicked)
                {
                    await AnalyzeCallbackCommand(client, update);
                    await client.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                }
            }
            if (update.Message != null && update.Message.Type == MessageType.Text ||
                update.Message != null && update.Message.Type == MessageType.Photo)
            {
                bool isKicked = await _kickTlgUserCommand
                        .CheckTlgUserIsKicked(update.Message?.Chat.Id);

                if (!isKicked)
                {
                    await AnalyzeTextCommand(client, update);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
            await _exceptionNotification.SendExceptionNotification(client, ex.Message,
                _adminNotifications.ChatIds);
        }

    }

    private async Task AnalyzeTextCommand(ITelegramBotClient client, Update update)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            _logger.LogAction($"Получено сообщение \"{update.Message.Text}\" " +
                $"от пользователя №{chatId} username {update.Message.Chat.Username}");

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
        }
    }

    private async Task AnalyzeCallbackCommand(ITelegramBotClient client, Update update)
    {
        if (update.CallbackQuery != null && update.CallbackQuery.Data != null && update.CallbackQuery.Message != null)
        {
            _logger.LogAction($"Получена команда \"{update.CallbackQuery.Data}\" " +
                $"от пользователя №{update.CallbackQuery.Message.Chat.Id} username {update.CallbackQuery.Message.Chat.Username}");

            foreach (var command in _baseCallbackCommands)
            {
                if (command.Contains(update.CallbackQuery))
                {
                    await command.CallbackExecute(update, client);
                }
            }
        }
    }
}
