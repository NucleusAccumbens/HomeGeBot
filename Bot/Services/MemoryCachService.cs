using Bot.Exceptions;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot.Types;

namespace Bot.Common.Services;

public class MemoryCachService : IMemoryCacheService
{
    private readonly IMemoryCache _memoryCach;

    public MemoryCachService(IMemoryCache memoryCache)
    {
        _memoryCach = memoryCache;
    }

    public string? GetCommandStateFromMemoryCache(long chatId)
    {
        var result = _memoryCach.Get(chatId);

        if (result is not null and string)
        {
            return (string)result;
        }

        else return null;
    }

    public Client GetClientFromMemoryCache(long chatId)
    {
        var result = _memoryCach.Get(chatId + 1);

        if (result is not null and Client)
        {
            return (Client)result;
        }

        else throw new MemoryCacheException();
    }

    public int GetMessageIdFromMemoryCache(long chatId)
    {
        var result = _memoryCach.Get(chatId + 2);

        if (result is not null and int)
        {
            return (int)result;
        }

        else throw new MemoryCacheException();
    }

    public List<IAlbumInputMedia> GetMediaGroupFromMemoryCache(long chatId)
    {
        var result = _memoryCach.Get(chatId + 3);

        if (result is not null and List<IAlbumInputMedia>)
        {
            return (List<IAlbumInputMedia>)result;
        }

        else throw new MemoryCacheException();
    }

    public byte GetMediaCountFromMemoryCache(long chatId)
    {
        var result = _memoryCach.Get(chatId + 4);

        if (result is not null and byte)
        {
            return (byte)result;
        }

        else return 0;
    }

    public void SetMemoryCache(long chatId, string commandState)
    {
        _memoryCach.Set(chatId, commandState,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
    }

    public void SetMemoryCache(long chatId, Client client)
    {
        _memoryCach.Set(chatId + 1, client,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
    }

    public void SetMemoryCache(long chatId, int messageId)
    {
        _memoryCach.Set(chatId + 2, messageId,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
    }

    public void SetMemoryCache(long chatId, IAlbumInputMedia media)
    {
        var result = _memoryCach.Get(chatId + 3);

        if (result == null && result is List<IAlbumInputMedia>)
        {
            _memoryCach.Set(chatId + 3, new List<IAlbumInputMedia>() { media }, 
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });

            return;
        }           
        if (result is not null and List<IAlbumInputMedia>)
        {
            (result as List<IAlbumInputMedia>).Add(media);

            _memoryCach.Set(chatId + 3, result,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });
        }
    }

    public void SetMemoryCache(long chatId, byte mediaCount)
    {
        _memoryCach.Set(chatId + 4, mediaCount,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });
    }

    public void RemoveCommandStateFromMemoryCache(long chatId)
    {
        _memoryCach.Remove(chatId);
    }

    public void RemoveClienteFromMemoryCache(long chatId)
    {
        _memoryCach.Remove(chatId + 1);
    }

    public void RemoveMessageIdFromMemoryCache(long chatId)
    {
        _memoryCach.Remove(chatId + 2);
    }

    public void RemoveMediaGroupFromMemoryCache(long chatId)
    {
        _memoryCach.Remove(chatId + 3);
    }    

    public void RemoveMediaCountFromMemoryCache(long chatId)
    {
        _memoryCach.Remove(chatId + 4);
    }
}
