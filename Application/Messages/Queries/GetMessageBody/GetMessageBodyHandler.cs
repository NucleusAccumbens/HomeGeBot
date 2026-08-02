using Microsoft.Extensions.Caching.Memory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Messages.Queries.GetMessageBody;

public class GetMessageBodyHandler : IRequestHandler<GetMessageBodyQuery, string?>
{
    private readonly IBotDbContext _context;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public GetMessageBodyHandler(IBotDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<string?> Handle(GetMessageBodyQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"message:{request.MessageName}:{request.Language}";

        if (_cache.TryGetValue(cacheKey, out string? cached))
        {
            return cached;
        }

        var message = await _context.Messages
            .Where(m => m.Name == request.MessageName)
            .Select(m => new { m.Body, m.BodyEn, m.BodyKa })
            .FirstOrDefaultAsync(cancellationToken);

        if (message == null) return null;

        var result = request.Language switch
        {
            "en" => !string.IsNullOrEmpty(message.BodyEn) ? message.BodyEn : message.Body,
            "ka" => !string.IsNullOrEmpty(message.BodyKa) ? message.BodyKa : message.Body,
            _ => message.Body
        };

        _cache.Set(cacheKey, result, CacheDuration);

        return result;
    }
}
