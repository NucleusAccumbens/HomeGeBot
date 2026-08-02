namespace Web.Services;

public interface ITmaInitDataParser
{
    IReadOnlyDictionary<string, string> Parse(string initData);
    TmaUserData? GetUserData(IReadOnlyDictionary<string, string> data);
}

public record TmaUserData(long Id, string? Username, string? FirstName, string? LastName, string? PhotoUrl);
