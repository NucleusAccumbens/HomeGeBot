namespace Application.Dashboard;

public interface IUpdateFlatCommentUseCase
{
    Task<UpdateFlatCommentResult> ExecuteAsync(UpdateFlatCommentRequest request);
}
