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
        var counts = await _context.Admins
            .Where(a => a.IsActive == true)
            .Include(a => a.Clients)
            .Select(a => new
            {
                Count = a.Clients.Count,
                ChatId = a.ChatId
            })
            .ToListAsync();

        var min = counts.OrderBy(c => c.Count).FirstOrDefault();

        if (min != null)
        {
            return min.ChatId;
        }

        return -1; 
    }

}
