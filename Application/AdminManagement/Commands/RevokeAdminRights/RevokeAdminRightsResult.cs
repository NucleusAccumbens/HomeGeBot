namespace Application.AdminManagement.Commands.RevokeAdminRights;

public class RevokeAdminRightsResult
{
    public static RevokeAdminRightsResult Success() => new();

    public static RevokeAdminRightsResult Failure(string error) => new() { ErrorMessage = error };

    public string? ErrorMessage { get; set; }
}
