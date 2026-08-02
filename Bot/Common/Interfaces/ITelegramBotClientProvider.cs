using Telegram.Bot;

namespace Bot.Common.Interfaces;

public interface ITelegramBotClientProvider
{
    Task<TelegramBotClient> GetBot();
}
