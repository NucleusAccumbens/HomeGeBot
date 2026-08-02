using Domain.Enums;

namespace Bot.Session;

public class RentalApplicationDraft
{
    public Country? Country { get; set; }
    public string? CountryOther { get; set; }

    public string? Profession { get; set; }

    public bool? HasPets { get; set; }

    public Term? Term { get; set; }
    public string? TermOther { get; set; }
}
