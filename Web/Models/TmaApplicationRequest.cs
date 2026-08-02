using Domain.Enums;

namespace Web.Models;

public class TmaApplicationRequest
{
    public string InitData { get; set; } = string.Empty;
    public Country Country { get; set; }
    public string? CountryOther { get; set; }
    public string Profession { get; set; } = string.Empty;
    public bool HasPets { get; set; }
    public Term Term { get; set; }
    public string? TermOther { get; set; }
}
