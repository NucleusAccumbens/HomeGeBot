using Application.TlgUsers.Commands.ToggleUserKick;
using Application.TlgUsers.Queries.CheckUserStatus;
using Bot.Common.Abstractions;
using Bot.Configuration;
using Bot.Messages.ClientMessages;
using Bot.Session;
using Microsoft.Extensions.Logging;
using MediatR;
using Microsoft.Extensions.Options;

namespace Bot.Common;

public class CommandAnalyzer : ICommandAnalyzer
{
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ILogger<CommandAnalyzer> _logger;
    private readonly IEnumerable<BaseTextCommand> _baseTextCommands;
    private readonly IEnumerable<BaseCallbackCommand> _baseCallbackCommands;
    private readonly IBotSessionStore _sessionStore;
    private readonly IMediator _mediator;
    private readonly AdminNotificationConfiguration _adminNotifications;
    private readonly ClientStartMessage _clientStartMessage;

    public CommandAnalyzer(IEnumerable<BaseTextCommand> textCommands,
        IEnumerable<BaseCallbackCommand> callbackCommands,
        IBotSessionStore sessionStore,
        IMediator mediator, IExceptionNotification exceptionNotification,
        ILogger<CommandAnalyzer> logger, IOptions<AdminNotificationConfiguration> adminNotifications,
        ClientStartMessage clientStartMessage)
    {
        _baseTextCommands = textCommands;
        _baseCallbackCommands = callbackCommands;
        _sessionStore = sessionStore;
        _mediator = mediator;
        _exceptionNotification = exceptionNotification;
        _logger = logger;
        _adminNotifications = adminNotifications.Value;
        _clientStartMessage = clientStartMessage;
    }

    public async Task AnalyzeCommandsAsync(ITelegramBotClient client, Update update)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.MyChatMember when update.MyChatMember != null:
                    await _mediator.Send(new ToggleUserKickCommand(update.MyChatMember.Chat.Id));
                    break;

                case UpdateType.CallbackQuery when update.CallbackQuery != null:
                    if (!await IsUserKicked(update.CallbackQuery.Message?.Chat.Id))
                    {
                        await AnalyzeCallbackCommand(client, update);
                        await client.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                    }
                    break;

                case UpdateType.Message when IsTextPhotoOrVideo(update.Message):
                    if (!await IsUserKicked(update.Message?.Chat.Id))
                    {
                        await AnalyzeTextCommand(client, update);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AnalyzeCommandsAsync");
            await _exceptionNotification.SendExceptionNotification(client,
                $"[{ex.GetType().Name}] {ex.Message}",
                _adminNotifications.ChatIds);
        }
    }

    private async Task<bool> IsUserKicked(long? chatId)
    {
        if (chatId == null) return false;
        return await _mediator.Send(new CheckUserStatusQuery(chatId));
    }

    private static bool IsTextPhotoOrVideo(Message? message)
    {
        return message != null &&
               (message.Type == MessageType.Text ||
                message.Type == MessageType.Photo ||
                message.Type == MessageType.Video);
    }

    private async Task AnalyzeTextCommand(ITelegramBotClient client, Update update)
    {
        if (update.Message == null) return;

        long chatId = update.Message.Chat.Id;

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

        // If no command matched, redirect to TMA (but ignore forwarded posts
        // that arrive as leftover media group items after session is cleared)
        if (update.Message.ForwardFromChat == null)
        {
            await _clientStartMessage.SendMessage(chatId, client);
        }
    }

    private async Task AnalyzeCallbackCommand(ITelegramBotClient client, Update update)
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
