namespace Application.Common.Localization;

public static class SupportedLanguages
{
    public const string Default = "ru";

    public static readonly string[] All = { "ru", "en", "ka" };

    public static bool IsValid(string? language)
    {
        return !string.IsNullOrEmpty(language) && All.Contains(language);
    }
}
