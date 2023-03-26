using Domain.Common;

namespace Domain.Entities;

public class HasPets : BaseEntity
{
    public string Yes { get; set; } = "Да";
    public string No { get; set; } = "Нет";
}
