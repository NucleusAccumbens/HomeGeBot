using Application.Common.Interfaces;
using Application.Common.Localization;
using Application.Common.Results;
using Application.Users.Queries.GetUserLanguage;
using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.RentalApplications.Commands.SubmitRentalApplication;

public class SubmitRentalApplicationHandler : IRequestHandler<SubmitRentalApplicationRequest, Result<SubmitRentalApplicationResult>>
{
    private const int MaxApplicationsPerUser = 5;

    private readonly IBotDbContext _context;
    private readonly IMediator _mediator;

    public SubmitRentalApplicationHandler(IBotDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<Result<SubmitRentalApplicationResult>> Handle(SubmitRentalApplicationRequest request, CancellationToken cancellationToken)
    {
        var existingCount = await _context.Clients
            .CountAsync(c => c.ChatId == request.ChatId, cancellationToken);

        if (existingCount >= MaxApplicationsPerUser)
        {
            var lang = await _mediator.Send(new GetUserLanguageQuery(request.ChatId), cancellationToken);
            return Result<SubmitRentalApplicationResult>.Failure(ApplicationMessages.ApplicationLimitExceeded(lang));
        }

        var manager = await _context.Admins
            .Where(a => a.IsActive)
            .Include(a => a.Clients)
            .OrderBy(a => a.Clients.Count)
            .FirstOrDefaultAsync(cancellationToken);

        if (manager == null)
        {
            var lang = await _mediator.Send(new GetUserLanguageQuery(request.ChatId), cancellationToken);
            return Result<SubmitRentalApplicationResult>.Failure(ApplicationMessages.NoActiveManagers(lang));
        }

        var client = new Client
        {
            ChatId = request.ChatId,
            Country = request.Country,
            CountryOther = request.CountryOther,
            Profession = request.Profession,
            HasPets = request.HasPets,
            Term = request.Term,
            TermOther = request.TermOther,
            AdminChatId = manager.ChatId
        };

        manager.Clients.Add(client);

        await _context.Clients.AddAsync(client, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        string? clientUsername = await _context.TlgUsers
            .Where(u => u.ChatId == request.ChatId)
            .Select(u => u.Username)
            .SingleOrDefaultAsync(cancellationToken);

        string? managerUsername = await _context.TlgUsers
            .Where(u => u.ChatId == manager.ChatId)
            .Select(u => u.Username)
            .SingleOrDefaultAsync(cancellationToken);

        return Result<SubmitRentalApplicationResult>.Success(
            SubmitRentalApplicationResult.Success(manager.ChatId, managerUsername, clientUsername));
    }
}
