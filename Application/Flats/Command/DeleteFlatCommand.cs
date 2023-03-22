using Application.Flats.Interfaces;

namespace Application.Flats.Command;

public class DeleteFlatCommand : IDeleteFlatCommand
{
    private readonly IBotDbContext _context;

    public DeleteFlatCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task DeleteFlatAsync(string itemId)
    {
        var flat = await _context.Flats
            .Where(f => f.ItemId== itemId)
            .SingleOrDefaultAsync();

        if (flat != null) 
        { 
            _context.Flats.Remove(flat);

            await _context.SaveChangesAsync();
        }
    }
}
