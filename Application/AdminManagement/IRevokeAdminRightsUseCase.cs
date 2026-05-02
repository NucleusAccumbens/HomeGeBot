namespace Application.AdminManagement;

public interface IRevokeAdminRightsUseCase
{
    Task<RevokeAdminRightsResult> ExecuteAsync(RevokeAdminRightsRequest request);
}
