using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Messages.Queries.GetMessageBody;

public class GetMessageBodyHandler : IRequestHandler<GetMessageBodyQuery, string?>
{
    private readonly IBotDbContext _context;

    public GetMessageBodyHandler(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<string?> Handle(GetMessageBodyQuery request, CancellationToken cancellationToken)
    {
        var message = await _context.Messages
            .Where(m => m.Name == request.MessageName)
            .Select(m => new { m.Body, m.BodyEn, m.BodyKa })
            .FirstOrDefaultAsync(cancellationToken);

        if (message == null) return null;

        return request.Language switch
        {
            "en" => !string.IsNullOrEmpty(message.BodyEn) ? message.BodyEn : message.Body,
            "ka" => !string.IsNullOrEmpty(message.BodyKa) ? message.BodyKa : message.Body,
            _ => message.Body
        };
    }
}
