namespace Application.AdminManagement;

public class RevokeAdminRightsUseCase : IRevokeAdminRightsUseCase
{
    private readonly IBotDbContext _context;

    public RevokeAdminRightsUseCase(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<RevokeAdminRightsResult> ExecuteAsync(RevokeAdminRightsRequest request)
    {
        var superAdmin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.SuperAdminChatId && a.IsActive);

        if (superAdmin == null)
        {
            return new RevokeAdminRightsResult
            {
                Success = false,
                ErrorMessage = "Доступ запрещён. Только администраторы могут отзывать права."
            };
        }

        if (request.TargetAdminChatId == request.SuperAdminChatId)
        {
            return new RevokeAdminRightsResult
            {
                Success = false,
                ErrorMessage = "Нельзя отозвать права у самого себя."
            };
        }

        var targetAdmin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.TargetAdminChatId && a.IsActive);

        if (targetAdmin == null)
        {
            return new RevokeAdminRightsResult
            {
                Success = false,
                ErrorMessage = "Администратор не найден или уже деактивирован."
            };
        }

        targetAdmin.IsActive = false;

        var targetUser = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == request.TargetAdminChatId);

        if (targetUser != null)
        {
            targetUser.IsAdmin = false;
        }

        await _context.SaveChangesAsync();

        return new RevokeAdminRightsResult
        {
            Success = true
        };
    }
}
