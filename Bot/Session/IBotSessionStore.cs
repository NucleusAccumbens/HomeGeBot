namespace Bot.Session;

public interface IBotSessionStore
{
    Task<BotSession?> GetAsync(long chatId);

    Task SaveAsync(BotSession session);

    Task ClearAsync(long chatId);
}
