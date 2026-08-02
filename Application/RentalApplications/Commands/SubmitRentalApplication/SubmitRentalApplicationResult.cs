using Domain.Common;

namespace Application.RentalApplications.Commands.SubmitRentalApplication;

public class SubmitRentalApplicationResult
{
    public static SubmitRentalApplicationResult Success(ChatId managerChatId, string? managerUsername, string? clientUsername) => new()
    {
        AssignedManagerChatId = managerChatId,
        ManagerUsername = managerUsername,
        ClientUsername = clientUsername
    };

    public static SubmitRentalApplicationResult Failure(string error) => new() { ErrorMessage = error };

    public ChatId? AssignedManagerChatId { get; set; }

    public string? ManagerUsername { get; set; }

    public string? ClientUsername { get; set; }

    public string? ErrorMessage { get; set; }
}
