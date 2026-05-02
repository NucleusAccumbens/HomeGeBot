namespace Application.Dashboard;

public class DeleteFlatUseCase : IDeleteFlatUseCase
{
    private readonly IBotDbContext _context;

    public DeleteFlatUseCase(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteFlatResult> ExecuteAsync(DeleteFlatRequest request)
    {
        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.AdminChatId && a.IsActive);

        if (admin == null)
        {
            return new DeleteFlatResult
            {
                Success = false,
                ErrorMessage = "Доступ запрещён."
            };
        }

        var flat = await _context.Flats
            .SingleOrDefaultAsync(f => f.ItemId == request.ItemId);

        if (flat == null)
        {
            return new DeleteFlatResult
            {
                Success = false,
                ErrorMessage = "Квартира не найдена."
            };
        }

        _context.Flats.Remove(flat);
        await _context.SaveChangesAsync();

        return new DeleteFlatResult
        {
            Success = true
        };
    }
}
