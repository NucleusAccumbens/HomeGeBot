namespace Application.AdminManagement;

public class GrantAdminRightsUseCase : IGrantAdminRightsUseCase
{
    private readonly IBotDbContext _context;

    public GrantAdminRightsUseCase(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<GrantAdminRightsResult> ExecuteAsync(GrantAdminRightsRequest request)
    {
        var superAdmin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.SuperAdminChatId && a.IsActive);

        if (superAdmin == null)
        {
            return new GrantAdminRightsResult
            {
                Success = false,
                ErrorMessage = "Доступ запрещён. Только администраторы могут назначать права."
            };
        }

        var targetUser = await _context.TlgUsers
            .SingleOrDefaultAsync(u => u.ChatId == request.TargetUserChatId);

        if (targetUser == null)
        {
            return new GrantAdminRightsResult
            {
                Success = false,
                ErrorMessage = "Пользователь не найден."
            };
        }

        if (targetUser.IsAdmin == true)
        {
            return new GrantAdminRightsResult
            {
                Success = false,
                ErrorMessage = "Пользователь уже является администратором."
            };
        }

        var existingAdmin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.TargetUserChatId);

        if (existingAdmin != null)
        {
            existingAdmin.IsActive = true;
        }
        else
        {
            var newAdmin = new Admin
            {
                ChatId = request.TargetUserChatId,
                IsActive = true,
                Clients = new List<Client>(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Admins.AddAsync(newAdmin);
        }

        targetUser.IsAdmin = true;

        await _context.SaveChangesAsync();

        return new GrantAdminRightsResult
        {
            Success = true
        };
    }
}
