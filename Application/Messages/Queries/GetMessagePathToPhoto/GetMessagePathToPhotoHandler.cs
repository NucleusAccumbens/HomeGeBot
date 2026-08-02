using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Messages.Queries.GetMessagePathToPhoto;

public class GetMessagePathToPhotoHandler : IRequestHandler<GetMessagePathToPhotoQuery, string?>
{
    private readonly IBotDbContext _context;

    public GetMessagePathToPhotoHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<string?> Handle(GetMessagePathToPhotoQuery request, CancellationToken cancellationToken)
    {
        return await _context.Messages
            .Where(m => m.Name == request.MessageName)
            .Select(m => m.PathToPhoto)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
