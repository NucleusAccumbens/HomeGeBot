namespace Application.BotStart;

public class StartBotUseCase : IStartBotUseCase
{
    private readonly IBotDbContext _context;

    public StartBotUseCase(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<StartBotResult> ExecuteAsync(StartBotRequest request)
    {
        var user = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == request.ChatId);

        if (user == null)
        {
            user = new TlgUser()
            {
                ChatId = request.ChatId,
                Username = request.Username,
                IsAdmin = false,
                IsKicked = false,
            };

            await _context.TlgUsers.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        else if (user.Username != request.Username && request.Username != null)
        {
            user.Username = request.Username;
            await _context.SaveChangesAsync();
        }

        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.ChatId && a.IsActive == true);

        return new StartBotResult()
        {
            IsAdmin = user.IsAdmin == true && admin != null,
            IsKicked = user.IsKicked,
        };
    }
}
