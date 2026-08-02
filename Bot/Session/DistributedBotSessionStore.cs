using Domain.Common;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace Bot.Session;

public class DistributedBotSessionStore : IBotSessionStore
{
    private readonly IDistributedCache _cache;

    public DistributedBotSessionStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<BotSession?> GetAsync(ChatId chatId)
    {
        var data = await _cache.GetAsync(GetKey(chatId));
        if (data == null)
            return null;

        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<BotSession>(json);
    }

    public async Task SaveAsync(BotSession session)
    {
        var json = JsonConvert.SerializeObject(session);
        var data = Encoding.UTF8.GetBytes(json);
        
        await _cache.SetAsync(GetKey(session.ChatId), data,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
    }

    public async Task ClearAsync(ChatId chatId)
    {
        await _cache.RemoveAsync(GetKey(chatId));
    }

    private static string GetKey(ChatId chatId) => $"bot_session_{chatId.Value}";
}
