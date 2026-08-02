using Application.Common.Interfaces;
using Application.Common.Results;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.AdminManagement.Commands.RevokeAdminRights;

public class RevokeAdminRightsHandler : IRequestHandler<RevokeAdminRightsRequest, Result<RevokeAdminRightsResult>>
{
    private readonly IBotDbContext _context;
    private readonly IUserNotifier _notifier;

    public RevokeAdminRightsHandler(IBotDbContext context, IUserNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task<Result<RevokeAdminRightsResult>> Handle(RevokeAdminRightsRequest request, CancellationToken cancellationToken)
    {
        if (request.TargetAdminChatId == request.SuperAdminChatId)
        {
            return Result<RevokeAdminRightsResult>.Failure("Нельзя отозвать права у самого себя.");
        }

        Admin? targetAdmin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.TargetAdminChatId && a.IsActive, cancellationToken);

        if (targetAdmin == null)
        {
            return Result<RevokeAdminRightsResult>.Failure("Администратор не найден или уже деактивирован.");
        }

        if (targetAdmin.Role == AdminRole.SuperAdmin)
        {
            return Result<RevokeAdminRightsResult>.Failure("Нельзя отозвать права супер-администратора.");
        }

        targetAdmin.Deactivate();

        /* Redistribute non-completed requests to other active admins (including superadmin) */
        var openClients = await _context.Clients
            .Where(c => c.AdminChatId == request.TargetAdminChatId && !c.IsCompleted)
            .ToListAsync(cancellationToken);

        if (openClients.Count > 0)
        {
            var activeAdminChatIds = await _context.Admins
                .AsNoTracking()
                .Where(a => a.IsActive)
                .Select(a => a.ChatId)
                .ToListAsync(cancellationToken);

            if (activeAdminChatIds.Count > 0)
            {
                for (int i = 0; i < openClients.Count; i++)
                {
                    var adminChatId = activeAdminChatIds[i % activeAdminChatIds.Count];
                    openClients[i].ChangeManager(adminChatId);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _notifier.SendNotificationAsync(
            request.TargetAdminChatId,
            "Ваши права администратора были отозваны.");

        return Result<RevokeAdminRightsResult>.Success(RevokeAdminRightsResult.Success());
    }
}
