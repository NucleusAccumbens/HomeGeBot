using Bot.Common.Services;

namespace Bot.Exceptions;

public class MemoryCacheException : Exception
{
    private readonly string _text = "Прошло слишком много времени с предыдущего сеанса, " +
        "данные не сохранились.\n\n" +
        "Чтобы начать сначала, нажмите /start";

    public MemoryCacheException()
        : base()
    {
    }

    public async Task SendExceptionMessage(long chatId, ITelegramBotClient client)
    {
        await MessageService.SendMessage(chatId, client, _text, null);
    }
}

