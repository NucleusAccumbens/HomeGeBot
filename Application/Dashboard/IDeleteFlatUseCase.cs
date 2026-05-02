namespace Application.Dashboard;

public interface IDeleteFlatUseCase
{
    Task<DeleteFlatResult> ExecuteAsync(DeleteFlatRequest request);
}
