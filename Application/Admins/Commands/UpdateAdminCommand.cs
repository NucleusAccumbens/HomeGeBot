using Application.Admins.Interfaces;

namespace Application.Admins.Commands;

public class UpdateAdminCommand : IUpdateAdminCommand
{
    private readonly IBotDbContext _context;

    public UpdateAdminCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task AddClientToAdminAsync(long chatId, Client client)
    {
        var admin = await _context.Admins
            .Where(a => a.ChatId == chatId)
            .SingleOrDefaultAsync();

        if (admin != null)
        {
            admin.Clients.Add(client);

            await _context.SaveChangesAsync();
        }
    }
}
