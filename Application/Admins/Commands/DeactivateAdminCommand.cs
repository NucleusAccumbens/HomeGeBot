using Application.Admins.Interfaces;

namespace Application.Admins.Commands;

public class DeactivateAdminCommand : IDeactivateAdminCommand
{
    private readonly IBotDbContext _context;

    public DeactivateAdminCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task DeactivateAdminAsync(long chatId)
    {
        var admin = await _context.Admins
            .Where(a => a.ChatId == chatId)
            .SingleOrDefaultAsync();

        var tlgUser = await _context.TlgUsers
            .Where(u => u.ChatId == chatId)
            .SingleOrDefaultAsync();

        if (admin == null || tlgUser == null) { throw new NullReferenceException(); }

        tlgUser.IsAdmin = false;
        admin.IsActive = false;
        await _context.SaveChangesAsync();
    }
}