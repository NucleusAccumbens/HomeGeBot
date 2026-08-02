using Domain.Common;
using Domain.Enums;

namespace Application.Dashboard.Dtos;

public class ApplicationDto
{
    public long Id { get; set; }
    public ChatId ChatId { get; set; }
    public string? Username { get; set; }

    public Country? Country { get; set; }

    public string? CountryOther { get; set; }

    public string? Profession { get; set; }

    public string? HasPets { get; set; }

    public Term? Term { get; set; }

    public string? TermOther { get; set; }

    public string? ManagerUsername { get; set; }
    public string? ManagerName { get; set; }
    public bool IsCompleted { get; set; }
}
