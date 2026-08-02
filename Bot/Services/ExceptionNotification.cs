using Domain.Common;

namespace Bot.Services;

public class ExceptionNotification : IExceptionNotification
{
    private readonly IMessageService _messageService;

    public ExceptionNotification(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task SendExceptionNotification(ITelegramBotClient client, string message, params ChatId[] chatIds)
    {
        foreach (var chatId in chatIds)
        {
            await _messageService.SendMessage(chatId, client,
                $"Вызвано исключение: {message}", null);
        }
    }
}
