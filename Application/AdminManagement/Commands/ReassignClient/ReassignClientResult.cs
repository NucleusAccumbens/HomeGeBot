using Application.Common.Results;

namespace Application.AdminManagement.Commands.ReassignClient;

public class ReassignClientResult
{
    public static ReassignClientResult Success() => new();
    public static ReassignClientResult Failure(string error) => new() { ErrorMessage = error };

    public string? ErrorMessage { get; set; }
}
