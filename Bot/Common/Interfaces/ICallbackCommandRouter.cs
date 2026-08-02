using Telegram.Bot.Types;

namespace Bot.Common.Interfaces;

public interface ICallbackCommandRouter
{
    Task RouteAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken);
}
