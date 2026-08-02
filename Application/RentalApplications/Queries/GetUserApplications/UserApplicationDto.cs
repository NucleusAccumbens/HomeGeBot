using Domain.Enums;

namespace Application.RentalApplications.Queries.GetUserApplications;

public class UserApplicationDto
{
    public Country? Country { get; set; }
    public string? CountryOther { get; set; }
    public string? Profession { get; set; }
    public bool? HasPets { get; set; }
    public Term? Term { get; set; }
    public string? TermOther { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ManagerUsername { get; set; }
}
