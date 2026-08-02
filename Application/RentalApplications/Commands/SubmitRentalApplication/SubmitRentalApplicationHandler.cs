using Application.Common.Interfaces;
using Application.Common.Results;
using Application.Users.Queries.GetUserLanguage;
using Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.RentalApplications.Commands.SubmitRentalApplication;

public class SubmitRentalApplicationHandler : IRequestHandler<SubmitRentalApplicationRequest, Result<SubmitRentalApplicationResult>>
{
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

        if (existingCount >= 5)
        {
            var lang = await _mediator.Send(new GetUserLanguageQuery(request.ChatId), cancellationToken);
            var limitMsg = lang switch
            {
                "en" => "You have already submitted 5 applications — this is the maximum.",
                "ka" => "თქვენ უკვე გაგზავნეთ 5 განაცხადი — ეს მაქსიმალური რაოდენობაა.",
                _ => "Вы уже отправили 5 заявок — это максимальное количество."
            };
            return Result<SubmitRentalApplicationResult>.Failure(limitMsg);
        }

        var manager = await _context.Admins
            .Where(a => a.IsActive)
            .Include(a => a.Clients)
            .OrderBy(a => a.Clients.Count)
            .FirstOrDefaultAsync(cancellationToken);

        if (manager == null)
        {
            var lang = await _mediator.Send(new GetUserLanguageQuery(request.ChatId), cancellationToken);
            var noManagerMsg = lang switch
            {
                "en" => "No active managers available.",
                "ka" => "აქტიური მენეჯერები არ არის.",
                _ => "Нет активных менеджеров"
            };
            return Result<SubmitRentalApplicationResult>.Failure(noManagerMsg);
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
