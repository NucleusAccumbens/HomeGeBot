namespace Application.Dashboard;

public class GetAdminDashboardResult
{
    public bool Success { get; set; }

    public List<ApplicationDto> Applications { get; set; } = new();

    public List<FlatDto> Flats { get; set; } = new();

    public List<ManagerDto> Managers { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
