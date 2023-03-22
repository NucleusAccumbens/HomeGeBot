using Domain.Entities;

namespace Bot.Common.Interfaces;

public interface IMemoryCacheService
{
    void SetMemoryCache(long chatId, string commandState);
    void SetMemoryCache(long chatId, Client client);
    void SetMemoryCache(long chatId, int messageId);
    void SetMemoryCache(long chatId, IAlbumInputMedia media);
    void SetMemoryCache(long chatId, byte mediaCount);
    string? GetCommandStateFromMemoryCache(long chatId);
    Client GetClientFromMemoryCache(long chatId);
    int GetMessageIdFromMemoryCache(long chatId);
    List<IAlbumInputMedia> GetMediaGroupFromMemoryCache(long chatId);
    byte GetMediaCountFromMemoryCache(long chatId);
    void RemoveCommandStateFromMemoryCache(long chatId);
    void RemoveClienteFromMemoryCache(long chatId);
    void RemoveMessageIdFromMemoryCache(long chatId);
    void RemoveMediaGroupFromMemoryCache(long chatId);
    void RemoveMediaCountFromMemoryCache(long chatId);
}
