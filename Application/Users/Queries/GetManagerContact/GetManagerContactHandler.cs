using Application.Common.Interfaces;
using Application.Common.Results;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.GetManagerContact;

public class GetManagerContactHandler : IRequestHandler<GetManagerContactQuery, Result<GetManagerContactResult>>
{
    private readonly IBotDbContext _context;

    public GetManagerContactHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetManagerContactResult>> Handle(GetManagerContactQuery request, CancellationToken cancellationToken)
    {
        var superAdmin = await _context.Admins
            .AsNoTracking()
            .Where(a => a.Role == AdminRole.SuperAdmin && a.IsActive)
            .Select(a => new { a.ChatId })
            .FirstOrDefaultAsync(cancellationToken);

        if (superAdmin == null)
        {
            return Result<GetManagerContactResult>.Failure("Супер-администратор не найден.");
        }

        var username = await _context.TlgUsers
            .AsNoTracking()
            .Where(u => u.ChatId == superAdmin.ChatId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<GetManagerContactResult>.Success(GetManagerContactResult.Success(username));
    }
}
