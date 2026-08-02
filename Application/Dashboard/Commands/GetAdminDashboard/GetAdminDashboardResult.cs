using Application.Dashboard.Dtos;

namespace Application.Dashboard.Commands.GetAdminDashboard;

public class GetAdminDashboardResult
{
    public static GetAdminDashboardResult Success(
        List<ApplicationDto> applications,
        List<FlatDto> flats,
        List<ManagerDto> managers,
        bool isSuperAdmin,
        string? currentAdminName = null,
        string? currentAdminUsername = null) => new()
    {
        Applications = applications,
        Flats = flats,
        Managers = managers,
        IsSuperAdmin = isSuperAdmin,
        CurrentAdminName = currentAdminName,
        CurrentAdminUsername = currentAdminUsername
    };

    public static GetAdminDashboardResult Failure(string error) => new() { ErrorMessage = error };

    public List<ApplicationDto> Applications { get; set; } = new();

    public List<FlatDto> Flats { get; set; } = new();

    public List<ManagerDto> Managers { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public bool IsSuperAdmin { get; set; }

    public string? CurrentAdminName { get; set; }

    public string? CurrentAdminUsername { get; set; }
}
