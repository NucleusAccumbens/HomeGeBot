using Domain.Enums;

namespace Application.RentalApplications.Queries.GetUserApplications;

public class GetUserApplicationsResult
{
    public static GetUserApplicationsResult Success(List<UserApplicationDto> applications) => new() { Applications = applications };

    public List<UserApplicationDto> Applications { get; set; } = new();
}
