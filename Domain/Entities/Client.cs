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
        Admin admin)
    {
        ChatId = chatId;
        Country = country;
        CountryOther = countryOther;
        Profession = profession;
        HasPets = hasPets;
        Term = term;
        TermOther = termOther;
        Admin = admin;
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

    public long AdminId { get; private set; }
    public Admin Admin { get; private set; } = null!;

    public bool IsCompleted { get; private set; }

    public void Complete() => IsCompleted = true;

    public void ChangeManager(Admin admin)
    {
        Admin = admin;
        AdminId = admin.Id;
    }

    public void UpdateDetails(string? profession, bool? hasPets)
    {
        Profession = profession;
        HasPets = hasPets;
    }
}
