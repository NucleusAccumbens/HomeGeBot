using Application.Common.Interfaces;
using Bot.Common;
using Microsoft.Extensions.Logging;

namespace Bot.Services;

public class UserNotifier : IUserNotifier
{
    private readonly TelegramBot _telegramBot;
    private readonly ILogger<UserNotifier> _logger;

    public UserNotifier(TelegramBot telegramBot, ILogger<UserNotifier> logger)
    {
        _telegramBot = telegramBot;
        _logger = logger;
    }

    public async Task SendNotificationAsync(Domain.Common.ChatId chatId, string message)
    {
        try
        {
            var client = await _telegramBot.GetBot();
            await MessageService.SendMessage(chatId, client, message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {ChatId}", chatId);
        }
    }
}
