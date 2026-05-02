namespace Application.RentalApplications;

public interface ISubmitRentalApplicationUseCase
{
    Task<SubmitRentalApplicationResult> ExecuteAsync(SubmitRentalApplicationRequest request);
}
