namespace Application.Users.Queries.GetManagerContact;

public class GetManagerContactResult
{
    public static GetManagerContactResult Success(string? username) => new() { Username = username };

    public string? Username { get; init; }
}
