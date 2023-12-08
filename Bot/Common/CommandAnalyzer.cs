using Application.TlgUsers.Interfaces;
using Bot.Common.Abstractions;
using Logger;
using Logger.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Common;

public class CommandAnalyzer : ICommandAnalyzer
{
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ICustomLogger _logger = new CustomLogger();   
    private readonly List<BaseTextCommand> _baseTextCommands;
    private readonly List<BaseCallbackCommand> _baseCallbackCommands;
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly IKickTlgUserCommand _kickTlgUserCommand;

    public CommandAnalyzer(IServiceProvider serviceProvider, IMemoryCacheService memoryCachService,
        IKickTlgUserCommand kickTlgUserCommand, IExceptionNotification exceptionNotification)
    {
        _baseTextCommands = serviceProvider.GetServices<BaseTextCommand>().ToList();
        _baseCallbackCommands = serviceProvider.GetServices<BaseCallbackCommand>().ToList();
        _memoryCacheService = memoryCachService;
        _kickTlgUserCommand = kickTlgUserCommand;
        _exceptionNotification = exceptionNotification;
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
                444343256, 2030541425);
        }

    }

    private async Task AnalyzeTextCommand(ITelegramBotClient client, Update update)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            _logger.LogAction($"Получено сообщение \"{update.Message.Text}\" " +
                $"от пользователя №{chatId} username {update.Message.Chat.Username}");

            foreach (var command in _baseTextCommands)
            {
                if (command.Name == update.Message?.Text ||
                    _memoryCacheService.GetCommandStateFromMemoryCache(chatId) != null && _memoryCacheService.GetCommandStateFromMemoryCache(chatId).Contains(command.Name))
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
