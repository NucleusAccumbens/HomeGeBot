namespace Application.Dashboard.Commands.DeleteFlat;

public class DeleteFlatResult
{
    public static DeleteFlatResult Success() => new();
    public static DeleteFlatResult Failure(string error) => new() { ErrorMessage = error };

    public string? ErrorMessage { get; set; }
}
