namespace Application.Dashboard;

public interface IGetAdminDashboardUseCase
{
    Task<GetAdminDashboardResult> ExecuteAsync(GetAdminDashboardRequest request);
}
