using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TlgUsers.Commands.ToggleUserKick;

public class ToggleUserKickHandler : IRequestHandler<ToggleUserKickCommand>
{
    private readonly IBotDbContext _context;

    public ToggleUserKickHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ToggleUserKickCommand request, CancellationToken cancellationToken)
    {
        var tlgUser = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == request.ChatId, cancellationToken);

        if (tlgUser != null)
        {
            tlgUser.IsKicked = !tlgUser.IsKicked;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
