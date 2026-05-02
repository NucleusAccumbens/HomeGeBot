namespace Application.AdminManagement;

public class GetBotUsersUseCase : IGetBotUsersUseCase
{
    private readonly IBotDbContext _context;

    public GetBotUsersUseCase(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<GetBotUsersResult> ExecuteAsync(GetBotUsersRequest request)
    {
        var requestorAdmin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.RequestorChatId && a.IsActive);

        if (requestorAdmin == null)
        {
            return new GetBotUsersResult
            {
                Success = false,
                ErrorMessage = "Доступ запрещён. Только администраторы могут просматривать пользователей."
            };
        }

        var query = _context.TlgUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var searchLower = request.SearchQuery.ToLower();
            query = query.Where(u => 
                (u.Username != null && u.Username.ToLower().Contains(searchLower)) ||
                u.ChatId.ToString().Contains(searchLower));
        }

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new BotUserDto
            {
                ChatId = u.ChatId,
                Username = u.Username,
                IsAdmin = u.IsAdmin == true,
                IsKicked = u.IsKicked,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new GetBotUsersResult
        {
            Success = true,
            Users = users
        };
    }
}
