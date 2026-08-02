namespace Application.TlgUsers.Commands.ToggleUserKick;

public class ToggleUserKickResult
{
    public bool IsKicked { get; init; }

    public static ToggleUserKickResult Success(bool isKicked) => new() { IsKicked = isKicked };
}
