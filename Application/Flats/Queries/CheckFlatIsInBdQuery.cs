using Application.Flats.Interfaces;

namespace Application.Flats.Queries;

public class CheckFlatIsInBdQuery : ICheckFlatIsInBdQuery
{
    private readonly IBotDbContext _context;

    public CheckFlatIsInBdQuery(IBotDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> CheckFlatIsInBdAsync(string id)
    {
        var flat = await _context.Flats
            .Where(f => f.ItemId == id)
            .SingleOrDefaultAsync();

        if (flat != null) return true;

        else return false;
    }
}
