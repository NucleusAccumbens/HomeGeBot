using Application.Clients.Interfaces;
using Application.Common.Exceptions;

namespace Application.Clients.Commands;

public class CreateClientCommand : ICreateClientCommand
{
    private readonly IBotDbContext _context;

    public CreateClientCommand(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Client> CreateClientAsync(Client client)
    {
        var thisClien = await _context.Clients
            .Where(c => c.ChatId == client.ChatId)
            .FirstOrDefaultAsync();

        if (thisClien != null) { client.AdminChatId = thisClien.AdminChatId; }

        await _context.Clients
            .AddAsync(client);

        await _context.SaveChangesAsync();

        return client;
    }
}
