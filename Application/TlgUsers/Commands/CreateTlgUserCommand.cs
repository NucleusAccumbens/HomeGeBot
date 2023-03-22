using Telegram.Bot.Types;

namespace Application.TlgUsers.Commands;

public class CreateTlgUserCommand : ICreateTlgUserCommand

{
    private readonly IBotDbContext _context;

    public CreateTlgUserCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task CreateTlgUserAsync(Chat chat)
    {
        var entity = new TlgUser()
        {
            ChatId = chat.Id,
            Username = chat.Username,
            IsAdmin = false,
            IsKicked = false,
        };

        await _context.TlgUsers.AddAsync(entity);

        await _context.SaveChangesAsync();
    }
}
