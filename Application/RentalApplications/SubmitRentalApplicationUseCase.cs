namespace Application.RentalApplications;

public class SubmitRentalApplicationUseCase : ISubmitRentalApplicationUseCase
{
    private readonly IBotDbContext _context;

    public SubmitRentalApplicationUseCase(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<SubmitRentalApplicationResult> ExecuteAsync(SubmitRentalApplicationRequest request)
    {
        var manager = await _context.Admins
            .Where(a => a.IsActive)
            .Include(a => a.Clients)
            .OrderBy(a => a.Clients.Count)
            .FirstOrDefaultAsync();

        if (manager == null)
        {
            return new SubmitRentalApplicationResult
            {
                Success = false,
                ErrorMessage = "Нет активных менеджеров"
            };
        }

        var client = new Client
        {
            ChatId = request.ChatId,
            Country = request.Country,
            Profession = request.Profession,
            HasPets = request.HasPets,
            Term = request.Term,
            AdminChatId = manager.ChatId
        };

        manager.Clients.Add(client);

        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        var username = await _context.TlgUsers
            .Where(u => u.ChatId == request.ChatId)
            .Select(u => u.Username)
            .SingleOrDefaultAsync();

        return new SubmitRentalApplicationResult
        {
            Success = true,
            AssignedManagerChatId = manager.ChatId,
            ClientUsername = username
        };
    }
}
