using Domain.Common;

namespace Bot.Session;

public interface IBotSessionStore
{
    Task<BotSession?> GetAsync(ChatId chatId);

    Task SaveAsync(BotSession session);

    Task ClearAsync(ChatId chatId);
}
