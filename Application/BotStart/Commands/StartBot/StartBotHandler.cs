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
            user = new TlgUser(
                chatId: request.ChatId,
                username: request.Username,
                firstName: request.FirstName,
                lastName: request.LastName);

            await _context.TlgUsers.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var newUsername = request.Username ?? user.Username;
            var newFirstName = request.FirstName ?? user.FirstName;
            var newLastName = request.LastName ?? user.LastName;

            if (newUsername != user.Username ||
                newFirstName != user.FirstName ||
                newLastName != user.LastName)
            {
                user.UpdateProfile(newUsername, newFirstName, newLastName);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.ChatId == request.ChatId && a.IsActive, cancellationToken);

        return Result<StartBotResult>.Success(StartBotResult.Create(
            isAdmin: admin != null,
            isSuperAdmin: admin != null && admin.Role == Domain.Enums.AdminRole.SuperAdmin,
            isKicked: user.IsKicked));
    }
}
