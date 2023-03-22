using Application.Admins.Interfaces;

namespace Application.Admins.Commands;

public class CreateAdminCommand : ICreateAdminCommand
{
    private readonly IBotDbContext _context;

    public CreateAdminCommand(IBotDbContext context)
    {
        _context = context;
    }
    
    public async Task CreateAdminAsync(Admin admin)
    {
        var thisAdmin = await _context.Admins
            .Where(a => a.ChatId == admin.ChatId)
            .SingleOrDefaultAsync();

        if (thisAdmin == null) 
        {
            await _context.Admins.AddAsync(admin);

            await _context.SaveChangesAsync();
        }      
    }
}
