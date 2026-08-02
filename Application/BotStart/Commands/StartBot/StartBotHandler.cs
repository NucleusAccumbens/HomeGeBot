using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;

namespace Application.BotStart.Commands.StartBot;

public class StartBotHandler : IRequestHandler<StartBotRequest, Result<StartBotResult>>
{
    private readonly IBotDbContext _context;

    public StartBotHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StartBotResult>> Handle(StartBotRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.TlgUsers
            .FirstOrDefaultAsync(u => u.ChatId == request.ChatId, cancellationToken);

        if (user == null)
        {
            user = new TlgUser()
            {
                ChatId = request.ChatId,
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsAdmin = false,
                IsKicked = false,
            };

            await _context.TlgUsers.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var changed = false;
            if (user.Username != request.Username && request.Username != null)
            {
                user.Username = request.Username;
                changed = true;
            }
            if (user.FirstName != request.FirstName && request.FirstName != null)
            {
                user.FirstName = request.FirstName;
                changed = true;
            }
            if (user.LastName != request.LastName && request.LastName != null)
            {
                user.LastName = request.LastName;
                changed = true;
            }
            if (changed)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.ChatId == request.ChatId && a.IsActive, cancellationToken);

        if (admin != null && !user.IsAdmin)
        {
            user.IsAdmin = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<StartBotResult>.Success(StartBotResult.Create(
            isAdmin: admin != null,
            isSuperAdmin: admin != null && admin.Role == Domain.Enums.AdminRole.SuperAdmin,
            isKicked: user.IsKicked));
    }
}
