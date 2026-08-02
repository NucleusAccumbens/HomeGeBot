using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.AdminManagement.Commands.CloseRequest;

public class CloseRequestHandler : IRequestHandler<CloseRequestRequest, Result<CloseRequestResult>>
{
    private readonly IBotDbContext _context;

    public CloseRequestHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CloseRequestResult>> Handle(CloseRequestRequest request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == request.ClientId && c.AdminChatId == request.AdminChatId, cancellationToken);

        if (client == null)
        {
            return Result<CloseRequestResult>.Failure("Заявка не найдена.");
        }

        client.IsCompleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<CloseRequestResult>.Success(CloseRequestResult.Success());
    }
}
