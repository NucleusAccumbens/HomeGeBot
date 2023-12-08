namespace Bot.Common.Interfaces;

public interface IExceptionNotification
{
    Task SendExceptionNotification(ITelegramBotClient client, string message, params long[] chatIds);
}
