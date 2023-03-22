using Application.Clients.Interfaces;

namespace Application.Clients.Queries;

public class GetClientDependencyQuery : IGetClientDependencyQuery
{
    private readonly IBotDbContext _context;

    public GetClientDependencyQuery(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetClientsManaderUsernameAsync(long managerChatId)
    {
        return await _context.TlgUsers
            .Where(u => u.ChatId == managerChatId && u.IsAdmin == true)
            .Select(u => u.Username)
            .SingleOrDefaultAsync();
    }

    public async Task<string?> GetClientUsernameAsync(long chatId)
    {
        return await _context.TlgUsers
            .Where(u => u.ChatId == chatId)
            .Select(u => u.Username)
            .SingleOrDefaultAsync();
    }
}
