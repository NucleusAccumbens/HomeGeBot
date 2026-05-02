namespace Application.AdminManagement;

public interface IGetBotUsersUseCase
{
    Task<GetBotUsersResult> ExecuteAsync(GetBotUsersRequest request);
}
