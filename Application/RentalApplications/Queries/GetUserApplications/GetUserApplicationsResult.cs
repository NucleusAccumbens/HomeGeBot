using Domain.Enums;

namespace Application.RentalApplications.Queries.GetUserApplications;

public class GetUserApplicationsResult
{
    public static GetUserApplicationsResult Success(List<UserApplicationDto> applications) => new() { Applications = applications };
    public static GetUserApplicationsResult Failure(string error) => new() { ErrorMessage = error };

    public List<UserApplicationDto> Applications { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
