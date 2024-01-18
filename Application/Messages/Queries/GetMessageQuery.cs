namespace Application.Messages.Queries;

public class GetMessageQuery : IGetMessageQuery
{
    private readonly IBotDbContext _context;

    public GetMessageQuery(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetMessageAsync(string messageName)
    {
        return await _context.Messages
            .Where(m => m.Name == messageName)
            .SingleOrDefaultAsync();
    }

    public async Task<string?> GetMessageBodyAsync(string messageName)
    {
        return await _context.Messages
            .Where(m => m.Name == messageName)
            .Select(m => m.Body)
            .SingleOrDefaultAsync();
    }

    public async Task<string?> GetMessagePathToPhotoAsync(string messageName)
    {
        return await _context.Messages
            .Where(m => m.Name == messageName)
            .Select(m => m.PathToPhoto)
            .SingleOrDefaultAsync();
    }
}
