namespace Application.TlgUsers.Commands.ToggleUserKick;

public class ToggleUserKickResult
{
    public bool IsKicked { get; init; }
    public string? ErrorMessage { get; init; }

    public static ToggleUserKickResult Success(bool isKicked) => new() { IsKicked = isKicked };
    public static ToggleUserKickResult Failure(string error) => new() { ErrorMessage = error };
}
