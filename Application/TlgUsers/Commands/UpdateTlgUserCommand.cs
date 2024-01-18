using Domain.Entities;

namespace Application.TlgUsers.Commands;

public class UpdateTlgUserCommand : IUpdateTlgUserCommand
{
    private readonly IBotDbContext _context;

    public UpdateTlgUserCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task UpdateTlgUserIsAdminAsync(long chatId, bool isAdmin)
    {
        var user = await _context.TlgUsers
            .Where(u => u.ChatId == chatId)
            .SingleOrDefaultAsync();

        if (user != null) 
        { 
            user.IsAdmin = isAdmin;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateUsernameAsync(long chatId, string username)
    {
        var user = await _context.TlgUsers
            .Where(u => u.ChatId == chatId)
            .SingleOrDefaultAsync();

        if (user != null)
        {
            user.Username = username;
            await _context.SaveChangesAsync();
        }
    }
}
