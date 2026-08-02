using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;

namespace Application.AdminManagement.Commands.GrantAdminRights;

public class GrantAdminRightsHandler : IRequestHandler<GrantAdminRightsRequest, Result<GrantAdminRightsResult>>
{
    private readonly IBotDbContext _context;
    private readonly IUserNotifier _notifier;

    public GrantAdminRightsHandler(IBotDbContext context, IUserNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task<Result<GrantAdminRightsResult>> Handle(GrantAdminRightsRequest request, CancellationToken cancellationToken)
    {
        var targetUser = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == request.TargetUserChatId, cancellationToken);

        if (targetUser == null)
        {
            return Result<GrantAdminRightsResult>.Failure("Пользователь не найден.");
        }

        if (targetUser.IsAdmin)
        {
            return Result<GrantAdminRightsResult>.Failure("Пользователь уже является администратором.");
        }

        var existingAdmin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.TargetUserChatId, cancellationToken);

        if (existingAdmin != null)
        {
            existingAdmin.Activate();
        }
        else
        {
            var newAdmin = new Admin
            {
                ChatId = request.TargetUserChatId
            };

            await _context.Admins.AddAsync(newAdmin, cancellationToken);
        }

        targetUser.SetAdmin(true);

        await _context.SaveChangesAsync(cancellationToken);

        await _notifier.SendNotificationAsync(
            request.TargetUserChatId,
            "Вам были назначены права администратора.");

        return Result<GrantAdminRightsResult>.Success(GrantAdminRightsResult.Success());
    }
}
