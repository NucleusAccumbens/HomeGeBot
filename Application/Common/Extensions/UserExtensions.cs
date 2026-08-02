namespace Application.Common.Extensions;

public static class UserExtensions
{
    public static string? GetFullName(string? firstName, string? lastName)
    {
        var name = (firstName ?? "") + (string.IsNullOrWhiteSpace(lastName) ? "" : " " + lastName);
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }
}
