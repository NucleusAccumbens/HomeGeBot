using Domain.Common;

namespace Domain.Entities;

public class TlgUser : BaseAuditableEntity
{
    public TlgUser(ChatId chatId,
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        string language = "ru",
        bool isKicked = false)
    {
        ChatId = chatId;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        Language = language;
        IsKicked = isKicked;
    }

    private TlgUser() { }

    public ChatId ChatId { get; private set; }
    public string? Username { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public bool IsKicked { get; private set; }
    public string Language { get; private set; } = "ru";

    public void UpdateProfile(string? username, string? firstName, string? lastName)
    {
        Username = username;
        FirstName = firstName;
        LastName = lastName;
    }

    public void SetKicked(bool kicked) => IsKicked = kicked;

    public void SetLanguage(string language) => Language = language;
}
