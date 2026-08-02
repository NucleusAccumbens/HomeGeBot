namespace Application.AdminManagement.Commands.GrantAdminRights;

public class GrantAdminRightsResult
{
    public static GrantAdminRightsResult Success() => new();

    public static GrantAdminRightsResult Failure(string error) => new() { ErrorMessage = error };

    public string? ErrorMessage { get; set; }
}
