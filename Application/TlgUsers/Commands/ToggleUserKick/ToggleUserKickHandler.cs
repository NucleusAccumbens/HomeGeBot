using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.TlgUsers.Commands.ToggleUserKick;

public class ToggleUserKickHandler : IRequestHandler<ToggleUserKickCommand, Result<ToggleUserKickResult>>
{
    private readonly IBotDbContext _context;

    public ToggleUserKickHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ToggleUserKickResult>> Handle(ToggleUserKickCommand request, CancellationToken cancellationToken)
    {
        var tlgUser = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == request.ChatId, cancellationToken);

        if (tlgUser == null)
        {
            return Result<ToggleUserKickResult>.Failure("Пользователь не найден.");
        }

        tlgUser.IsKicked = !tlgUser.IsKicked;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ToggleUserKickResult>.Success(ToggleUserKickResult.Success(tlgUser.IsKicked));
    }
}
