namespace Application.Users.Queries.GetManagerContact;

public class GetManagerContactResult
{
    public static GetManagerContactResult Success(string? username) => new() { Username = username };
    public static GetManagerContactResult Failure(string error) => new() { ErrorMessage = error };

    public string? Username { get; init; }
    public string? ErrorMessage { get; init; }
}
