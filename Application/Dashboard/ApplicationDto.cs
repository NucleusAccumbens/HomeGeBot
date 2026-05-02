using Domain.Enums;

namespace Application.Dashboard;

public class ApplicationDto
{
    public long ChatId { get; set; }

    public string? Username { get; set; }

    public Country? Country { get; set; }

    public string? Profession { get; set; }

    public string? HasPets { get; set; }

    public Term? Term { get; set; }

    public string? ManagerUsername { get; set; }
}
