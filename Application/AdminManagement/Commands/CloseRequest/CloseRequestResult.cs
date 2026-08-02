using Application.Common.Results;

namespace Application.AdminManagement.Commands.CloseRequest;

public class CloseRequestResult
{
    public static CloseRequestResult Success() => new();
    public static CloseRequestResult Failure(string error) => new() { ErrorMessage = error };

    public string? ErrorMessage { get; set; }
}
