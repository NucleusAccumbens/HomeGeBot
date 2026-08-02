using Bot.Services;
using Domain.Common;

namespace Bot.Services;

public class ExceptionNotification : IExceptionNotification
{
    public async Task SendExceptionNotification(ITelegramBotClient client, string message, params ChatId[] chatIds)
    {
        foreach (var chatId in chatIds) 
        { 
            await MessageService.SendMessage(chatId, client, 
                $"Вызвано исключение: {message}", null);
        }
    }
}
