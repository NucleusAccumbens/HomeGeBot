using Telegram.Bot.Types;

namespace Bot.Common.Interfaces;

public interface ITextCommandRouter
{
    Task RouteAsync(ITelegramBotClient client, Update update, long chatId, CancellationToken cancellationToken);
}
