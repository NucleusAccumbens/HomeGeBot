using Domain.Enums;

namespace Application.RentalApplications;

public class SubmitRentalApplicationRequest
{
    public long ChatId { get; set; }

    public Country Country { get; set; }

    public string Profession { get; set; } = "";

    public bool HasPets { get; set; }

    public Term Term { get; set; }
}
