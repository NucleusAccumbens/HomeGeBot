using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Client : BaseAuditableEntity
{
    public Client(ChatId chatId,
        Country? country,
        string? countryOther,
        string? profession,
        bool? hasPets,
        Term? term,
        string? termOther,
        ChatId adminChatId)
    {
        ChatId = chatId;
        Country = country;
        CountryOther = countryOther;
        Profession = profession;
        HasPets = hasPets;
        Term = term;
        TermOther = termOther;
        AdminChatId = adminChatId;
        IsCompleted = false;
    }

    private Client() { }

    public ChatId ChatId { get; private set; }
    public Country? Country { get; private set; }
    public string? CountryOther { get; private set; }
    public string? Profession { get; private set; }
    public bool? HasPets { get; private set; }
    public Term? Term { get; private set; }
    public string? TermOther { get; private set; }
    public ChatId AdminChatId { get; private set; }
    public bool IsCompleted { get; private set; }

    public void Complete() => IsCompleted = true;

    public void ChangeManager(ChatId adminChatId) => AdminChatId = adminChatId;

    public void UpdateDetails(string? profession, bool? hasPets)
    {
        Profession = profession;
        HasPets = hasPets;
    }
}
