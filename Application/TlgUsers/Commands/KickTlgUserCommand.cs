namespace Application.TlgUsers.Commands;

public class KickTlgUserCommand : IKickTlgUserCommand
{
    private readonly IBotDbContext _context;

    public KickTlgUserCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckTlgUserIsKicked(long? chatId)
    {
        if (chatId == null) return true;

        var tlgUser = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == chatId);

        if (tlgUser != null)
        {
            return tlgUser.IsKicked;
        }

        return false;
    }

    public async Task ManageTlgUserKickingAsync(long chatId)
    {
        var tlgUser = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == chatId);

        if (tlgUser != null)
        {
            tlgUser.IsKicked = !tlgUser.IsKicked;

            await _context.SaveChangesAsync();
        }
    }
}
