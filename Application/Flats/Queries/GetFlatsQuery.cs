using Application.Flats.Interfaces;

namespace Application.Flats.Queries;

public class GetFlatsQuery : IGetFlatsQuery
{
    private readonly IBotDbContext _context;

    public GetFlatsQuery(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<List<Flat>> GetFlatsAsync()
    {
        return await _context.Flats
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}
