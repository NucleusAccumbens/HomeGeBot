namespace Application.AdminManagement;

public interface IGrantAdminRightsUseCase
{
    Task<GrantAdminRightsResult> ExecuteAsync(GrantAdminRightsRequest request);
}
