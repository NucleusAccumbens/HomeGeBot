using Bot.Session;
using Telegram.Bot.Types;

namespace Bot.Services;

public interface IRentalApplicationForwardProcessor
{
    Task<string?> ProcessAsync(long chatId, Update update, ITelegramBotClient client, BotSession session, CancellationToken cancellationToken = default);
}
