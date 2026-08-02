using Application.Common.Interfaces;
using Application.Common.Results;
using MediatR;

namespace Application.Dashboard.Commands.DeleteFlat;

public class DeleteFlatHandler : IRequestHandler<DeleteFlatRequest, Result<DeleteFlatResult>>
{
    private readonly IBotDbContext _context;

    public DeleteFlatHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DeleteFlatResult>> Handle(DeleteFlatRequest request, CancellationToken cancellationToken)
    {
        var flat = await _context.Flats
            .SingleOrDefaultAsync(f => f.ItemId == request.ItemId, cancellationToken);

        if (flat == null)
        {
            return Result<DeleteFlatResult>.Failure("Квартира не найдена.");
        }

        _context.Flats.Remove(flat);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<DeleteFlatResult>.Success(DeleteFlatResult.Success());
    }
}
