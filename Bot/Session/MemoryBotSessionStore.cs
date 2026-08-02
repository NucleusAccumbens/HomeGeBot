using Domain.Common;
using Microsoft.Extensions.Caching.Memory;

namespace Bot.Session;

public class MemoryBotSessionStore : IBotSessionStore
{
    private readonly IMemoryCache _cache;

    public MemoryBotSessionStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<BotSession?> GetAsync(ChatId chatId)
    {
        var session = _cache.Get<BotSession>(GetKey(chatId));
        return Task.FromResult(session);
    }

    public Task SaveAsync(BotSession session)
    {
        _cache.Set(GetKey(session.ChatId), session,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });

        return Task.CompletedTask;
    }

    public Task ClearAsync(ChatId chatId)
    {
        _cache.Remove(GetKey(chatId));
        return Task.CompletedTask;
    }

    private static string GetKey(ChatId chatId) => $"bot_session_{chatId.Value}";
}
