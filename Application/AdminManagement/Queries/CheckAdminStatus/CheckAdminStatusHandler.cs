using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.AdminManagement.Queries.CheckAdminStatus;

public class CheckAdminStatusHandler : IRequestHandler<CheckAdminStatusQuery, bool>
{
    private readonly IBotDbContext _context;

    public CheckAdminStatusHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CheckAdminStatusQuery request, CancellationToken cancellationToken)
    {
        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.ChatId && a.IsActive, cancellationToken);

        return admin != null;
    }
}
