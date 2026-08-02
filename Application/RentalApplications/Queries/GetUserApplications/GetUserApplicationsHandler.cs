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
        var applications = await _context.Clients
            .AsNoTracking()
            .Where(c => c.ChatId == request.ChatId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new UserApplicationDto
            {
                Country = c.Country,
                CountryOther = c.CountryOther,
                Profession = c.Profession,
                HasPets = c.HasPets,
                Term = c.Term,
                TermOther = c.TermOther,
                CreatedAt = c.CreatedAt,
                ManagerUsername = _context.TlgUsers
                    .Where(u => u.ChatId == c.AdminChatId)
                    .Select(u => u.Username)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return Result<GetUserApplicationsResult>.Success(GetUserApplicationsResult.Success(applications));
    }
}
