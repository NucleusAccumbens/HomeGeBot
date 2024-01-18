using Application.Admins.Interfaces;

namespace Application.Admins.Queries;

public class GetAdminsQuery : IGetAdminsQuery
{
    private readonly IBotDbContext _context;

    public GetAdminsQuery(IBotDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<long>> GetAdminsChatIdsAsync()
    {
        return await _context.TlgUsers
            .Where(u => u.IsAdmin == true && u.IsKicked == false)
            .Select(u => u.ChatId)
            .ToListAsync();
    }

    public async Task<long> GetAdminWithLeastClientCountAsync()
    {
        List<int> counts = await _context.Admins
            .Include(a => a.Clients)
            .Select(a => a.Clients.Count)
            .ToListAsync();

        var min = counts.Min();

        return await _context.Admins
            .Include(a => a.Clients)
            .Where(a => a.Clients.Count == min)
            .Select(a => a.ChatId)
            .FirstAsync();
    }
}
