using Application.Common.Interfaces;
using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.GetUserLanguage;

public class GetUserLanguageHandler : IRequestHandler<GetUserLanguageQuery, string>
{
    private readonly IBotDbContext _context;

    public GetUserLanguageHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetUserLanguageQuery request, CancellationToken cancellationToken)
    {
        var language = await _context.TlgUsers
            .Where(u => u.ChatId == new ChatId(request.ChatId))
            .Select(u => u.Language)
            .FirstOrDefaultAsync(cancellationToken);

        return language ?? "ru";
    }
}
