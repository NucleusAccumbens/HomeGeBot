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
            if (update.Type == UpdateType.MyChatMember && update.MyChatMember != null)
            {
                await _mediator.Send(new ToggleUserKickCommand(update.MyChatMember.Chat.Id));
            }
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                bool isKicked = await _mediator
                        .Send(new CheckUserStatusQuery(update.CallbackQuery.Message?.Chat.Id));

                if (!isKicked)
                {
                    await AnalyzeCallbackCommand(client, update);
                    await client.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                }
            }
            if ((update.Message != null && update.Message.Type == MessageType.Text) ||
                (update.Message != null && update.Message.Type == MessageType.Photo) ||
                (update.Message != null && update.Message.Type == MessageType.Video))
            {
                bool isKicked = await _mediator
                        .Send(new CheckUserStatusQuery(update.Message?.Chat.Id));

                if (!isKicked)
                {
                    await AnalyzeTextCommand(client, update);
                }
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

    private async Task AnalyzeTextCommand(ITelegramBotClient client, Update update)
    {
        if (update.Message != null)
        {
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
    }

    private async Task AnalyzeCallbackCommand(ITelegramBotClient client, Update update)
    {
        if (update.CallbackQuery != null && update.CallbackQuery.Data != null && update.CallbackQuery.Message != null)
        {
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
}
