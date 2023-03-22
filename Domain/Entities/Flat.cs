using Domain.Common;

namespace Domain.Entities;

public class Flat : BaseAuditableEntity
{
    public string? ItemId { get; set; }
    public string? Link { get; set; }
    public string? OwnerNumber { get; set; }
    public string? Comment { get; set; }
}
