using Application.Flats.Interfaces;

namespace Application.Flats.Command;

public class CreateFlatCommand : ICreateFlatCommand
{
    private readonly IBotDbContext _context;

    public CreateFlatCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task CreateFlatAsync(Flat flat)
    {
        await _context.Flats
            .AddAsync(flat);

        await _context.SaveChangesAsync();
    }
}
