using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TlgUsers.Queries.CheckUserStatus;

public class CheckUserStatusHandler : IRequestHandler<CheckUserStatusQuery, bool>
{
    private readonly IBotDbContext _context;

    public CheckUserStatusHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CheckUserStatusQuery request, CancellationToken cancellationToken)
    {
        if (request.ChatId == null) return true;

        var tlgUser = await _context.TlgUsers
            .FirstOrDefaultAsync(u => u.ChatId == request.ChatId, cancellationToken);

        if (tlgUser != null)
        {
            return tlgUser.IsKicked;
        }

        return false;
    }
}
