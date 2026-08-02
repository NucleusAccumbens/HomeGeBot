namespace Web.Services;

public interface ITmaValidationService
{
    bool ValidateInitData(string initData);
    long? GetUserId(string initData);
    string? GetUsername(string initData);
    TmaUserData? GetUserData(string initData);
}

public record TmaUserData(long Id, string? Username, string? FirstName, string? LastName, string? PhotoUrl);
