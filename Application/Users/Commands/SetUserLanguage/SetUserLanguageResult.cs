namespace Application.Users.Commands.SetUserLanguage;

public class SetUserLanguageResult
{
    public static SetUserLanguageResult Success() => new();
    public static SetUserLanguageResult Failure(string error) => new() { ErrorMessage = error };

    public string? ErrorMessage { get; init; }
}
