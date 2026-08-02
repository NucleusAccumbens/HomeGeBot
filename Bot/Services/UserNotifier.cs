using Application.Common.Interfaces;
using Bot.Common;
using Microsoft.Extensions.Logging;

namespace Bot.Services;

public class UserNotifier : IUserNotifier
{
    private readonly ITelegramBotClientProvider _telegramBot;
    private readonly ILogger<UserNotifier> _logger;
    private readonly IMessageService _messageService;

    public UserNotifier(ITelegramBotClientProvider telegramBot, ILogger<UserNotifier> logger, IMessageService messageService)
    {
        _telegramBot = telegramBot;
        _logger = logger;
        _messageService = messageService;
    }

    public async Task SendNotificationAsync(Domain.Common.ChatId chatId, string message)
    {
        try
        {
            var client = await _telegramBot.GetBot();
            await _messageService.SendMessage(chatId, client, message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {ChatId}", chatId);
        }
    }
}
