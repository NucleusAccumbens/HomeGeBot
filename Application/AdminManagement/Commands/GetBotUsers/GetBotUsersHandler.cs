using Application.AdminManagement.Dtos;
using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.AdminManagement.Commands.GetBotUsers;

public class GetBotUsersHandler : IRequestHandler<GetBotUsersRequest, Result<GetBotUsersResult>>
{
    private readonly IBotDbContext _context;

    public GetBotUsersHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetBotUsersResult>> Handle(GetBotUsersRequest request, CancellationToken cancellationToken)
    {
        var adminChatIds = await _context.Admins
            .AsNoTracking()
            .Where(a => a.IsActive)
            .Select(a => a.ChatId)
            .ToListAsync(cancellationToken);

        var query = _context.TlgUsers
            .Where(u => !adminChatIds.Contains(u.ChatId));

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
                Name = (u.FirstName != null ? u.FirstName : "") + (u.LastName != null ? " " + u.LastName : ""),
                IsAdmin = u.IsAdmin,
                IsKicked = u.IsKicked,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<GetBotUsersResult>.Success(GetBotUsersResult.Success(users));
    }
}
