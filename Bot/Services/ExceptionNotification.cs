using Bot.Common.Services;

namespace Bot.Services;

public class ExceptionNotification : IExceptionNotification
{
    public async Task SendExceptionNotification(ITelegramBotClient client, string message, params long[] chatIds)
    {
        foreach (var chatId in chatIds) 
        { 
            await MessageService.SendMessage(chatId, client, 
                $"Вызвано исключение: {message}", null);
        }
    }
}
