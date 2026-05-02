namespace Application.Dashboard;

public class GetAdminDashboardUseCase : IGetAdminDashboardUseCase
{
    private readonly IBotDbContext _context;

    public GetAdminDashboardUseCase(IBotDbContext context)
    {
        _context = context;
    }

    public async Task<GetAdminDashboardResult> ExecuteAsync(GetAdminDashboardRequest request)
    {
        var admin = await _context.Admins
            .SingleOrDefaultAsync(a => a.ChatId == request.AdminChatId && a.IsActive);

        if (admin == null)
        {
            return new GetAdminDashboardResult
            {
                Success = false,
                ErrorMessage = "Доступ запрещён. Только администраторы могут просматривать дашборд."
            };
        }

        var applications = await GetApplicationsAsync();
        var flats = await GetFlatsAsync();
        var managers = await GetManagersAsync();

        return new GetAdminDashboardResult
        {
            Success = true,
            Applications = applications,
            Flats = flats,
            Managers = managers
        };
    }

    private async Task<List<ApplicationDto>> GetApplicationsAsync()
    {
        var clients = await _context.Clients
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var applications = new List<ApplicationDto>();

        foreach (var client in clients)
        {
            var username = await _context.TlgUsers
                .Where(u => u.ChatId == client.ChatId)
                .Select(u => u.Username)
                .SingleOrDefaultAsync();

            var managerUsername = await _context.TlgUsers
                .Where(u => u.ChatId == client.AdminChatId)
                .Select(u => u.Username)
                .SingleOrDefaultAsync();

            applications.Add(new ApplicationDto
            {
                ChatId = client.ChatId,
                Username = username,
                Country = client.Country,
                Profession = client.Profession,
                HasPets = client.HasPets == true ? "Да" : "Нет",
                Term = client.Term,
                ManagerUsername = managerUsername
            });
        }

        return applications;
    }

    private async Task<List<FlatDto>> GetFlatsAsync()
    {
        var flats = await _context.Flats
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return flats.Select(f => new FlatDto
        {
            ItemId = f.ItemId ?? "",
            PublicationDate = f.CreatedAt.ToShortDateString(),
            Link = f.Link ?? "",
            OwnerNumber = f.OwnerNumber ?? "",
            Comment = f.Comment
        }).ToList();
    }

    private async Task<List<ManagerDto>> GetManagersAsync()
    {
        var admins = await _context.Admins
            .Include(a => a.Clients)
            .ToListAsync();

        var managers = new List<ManagerDto>();

        foreach (var admin in admins)
        {
            var username = await _context.TlgUsers
                .Where(u => u.ChatId == admin.ChatId)
                .Select(u => u.Username)
                .SingleOrDefaultAsync();

            managers.Add(new ManagerDto
            {
                ChatId = admin.ChatId,
                Username = username,
                ApplicationCount = admin.Clients.Count
            });
        }

        return managers;
    }
}
