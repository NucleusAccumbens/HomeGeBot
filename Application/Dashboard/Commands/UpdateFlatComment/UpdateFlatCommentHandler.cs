using Application.Common.Interfaces;
using Application.Common.Results;
using Application.Dashboard.Dtos;
using MediatR;

namespace Application.Dashboard.Commands.UpdateFlatComment;

public class UpdateFlatCommentHandler : IRequestHandler<UpdateFlatCommentRequest, Result<UpdateFlatCommentResult>>
{
    private readonly IBotDbContext _context;

    public UpdateFlatCommentHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateFlatCommentResult>> Handle(UpdateFlatCommentRequest request, CancellationToken cancellationToken)
    {
        var flat = await _context.Flats
            .SingleOrDefaultAsync(f => f.ItemId == request.ItemId, cancellationToken);

        if (flat == null)
        {
            return Result<UpdateFlatCommentResult>.Failure("Квартира не найдена.");
        }

        flat.Comment = request.Comment;
        await _context.SaveChangesAsync(cancellationToken);

        var flatDto = new FlatDto
        {
            ItemId = flat.ItemId ?? "",
            PublicationDate = flat.CreatedAt.ToShortDateString(),
            Link = flat.Link ?? "",
            OwnerNumber = flat.OwnerNumber ?? "",
            Comment = flat.Comment
        };

        return Result<UpdateFlatCommentResult>.Success(UpdateFlatCommentResult.Success(flatDto));
    }
}
