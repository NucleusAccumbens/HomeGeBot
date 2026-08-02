using Application.Common.Interfaces;
using Application.Common.Results;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.AdminManagement.Commands.ReassignClient;

public class ReassignClientHandler : IRequestHandler<ReassignClientRequest, Result<ReassignClientResult>>
{
    private readonly IBotDbContext _context;

    public ReassignClientHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReassignClientResult>> Handle(ReassignClientRequest request, CancellationToken cancellationToken)
    {
        var clients = await _context.Clients
            .Where(c => c.ChatId == request.ClientChatId && !c.IsCompleted)
            .ToListAsync(cancellationToken);

        if (clients.Count == 0)
        {
            return Result<ReassignClientResult>.Failure("Заявка не найдена или уже закрыта.");
        }

        var newManager = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.NewManagerChatId && a.IsActive, cancellationToken);

        if (newManager == null)
        {
            return Result<ReassignClientResult>.Failure("Менеджер не найден.");
        }

        foreach (var client in clients)
        {
            client.ChangeManager(newManager);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ReassignClientResult>.Success(ReassignClientResult.Success());
    }
}
