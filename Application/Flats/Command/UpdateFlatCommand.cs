using Application.Flats.Interfaces;

namespace Application.Flats.Command;

public class UpdateFlatCommand : IUpdateFlatCommand
{
    private readonly IBotDbContext _context;

    public UpdateFlatCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task UpdateCommentAsync(string itemId, string comment)
    {
        var flat = await _context.Flats
            .Where(f => f.ItemId == itemId)
            .SingleOrDefaultAsync();

        if (flat != null) 
        { 
            flat.Comment = comment;

            await _context.SaveChangesAsync();
        }
    }
}
