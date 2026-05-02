namespace Application.RentalApplications;

public class SubmitRentalApplicationResult
{
    public bool Success { get; set; }

    public long? AssignedManagerChatId { get; set; }

    public string? ClientUsername { get; set; }

    public string? ErrorMessage { get; set; }
}
