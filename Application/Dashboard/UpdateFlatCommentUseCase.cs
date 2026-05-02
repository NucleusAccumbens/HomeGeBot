namespace Application.Dashboard;

public class UpdateFlatCommentUseCase : IUpdateFlatCommentUseCase
{
    private readonly IBotDbContext _context;

    public UpdateFlatCommentUseCase(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateFlatCommentResult> ExecuteAsync(UpdateFlatCommentRequest request)
    {
        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.AdminChatId && a.IsActive);

        if (admin == null)
        {
            return new UpdateFlatCommentResult
            {
                Success = false,
                ErrorMessage = "Доступ запрещён."
            };
        }

        var flat = await _context.Flats
            .SingleOrDefaultAsync(f => f.ItemId == request.ItemId);

        if (flat == null)
        {
            return new UpdateFlatCommentResult
            {
                Success = false,
                ErrorMessage = "Квартира не найдена."
            };
        }

        flat.Comment = request.Comment;
        await _context.SaveChangesAsync();

        return new UpdateFlatCommentResult
        {
            Success = true,
            UpdatedFlat = new FlatDto
            {
                ItemId = flat.ItemId ?? "",
                PublicationDate = flat.CreatedAt.ToShortDateString(),
                Link = flat.Link ?? "",
                OwnerNumber = flat.OwnerNumber ?? "",
                Comment = flat.Comment
            }
        };
    }
}
