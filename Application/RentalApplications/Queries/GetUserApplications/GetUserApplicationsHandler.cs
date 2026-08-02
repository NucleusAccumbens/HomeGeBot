using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.RentalApplications.Queries.GetUserApplications;

public class GetUserApplicationsHandler : IRequestHandler<GetUserApplicationsRequest, Result<GetUserApplicationsResult>>
{
    private readonly IBotDbContext _context;

    public GetUserApplicationsHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetUserApplicationsResult>> Handle(GetUserApplicationsRequest request, CancellationToken cancellationToken)
    {
        var clients = await _context.Clients
            .AsNoTracking()
            .Where(c => c.ChatId == request.ChatId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Country,
                c.CountryOther,
                c.Profession,
                c.HasPets,
                c.Term,
                c.TermOther,
                c.CreatedAt,
                c.AdminChatId
            })
            .ToListAsync(cancellationToken);

        var adminChatIds = clients.Select(c => c.AdminChatId).Distinct().ToList();
        var usernames = await _context.TlgUsers
            .AsNoTracking()
            .Where(u => adminChatIds.Contains(u.ChatId))
            .ToDictionaryAsync(u => u.ChatId, u => u.Username, cancellationToken);

        var applications = clients
            .Select(c => new UserApplicationDto
            {
                Country = c.Country,
                CountryOther = c.CountryOther,
                Profession = c.Profession,
                HasPets = c.HasPets,
                Term = c.Term,
                TermOther = c.TermOther,
                CreatedAt = c.CreatedAt,
                ManagerUsername = usernames.GetValueOrDefault(c.AdminChatId)
            })
            .ToList();

        return Result<GetUserApplicationsResult>.Success(GetUserApplicationsResult.Success(applications));
    }
}
