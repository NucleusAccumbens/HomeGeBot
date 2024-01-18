namespace Application.TlgUsers.Queries;

public class CheckUserIsAdminQuery : ICheckUserIsAdminQuery
{
    private readonly IBotDbContext _context;

    public CheckUserIsAdminQuery(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<bool?> CheckUserIsAdminAsync(long chatId)
    {
        var user = await _context.TlgUsers
            .Where(u => u.ChatId== chatId)
            .SingleOrDefaultAsync();

        var admin = await _context.Admins
            .Where(a => a.ChatId== chatId)
            .SingleOrDefaultAsync();

        if (user != null && user.IsAdmin == true && admin != null)
        {
            return user.IsAdmin;
        }

        else return false;       
    }
}
