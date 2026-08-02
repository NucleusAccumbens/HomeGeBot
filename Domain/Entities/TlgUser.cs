using Domain.Common;

namespace Domain.Entities;

public class TlgUser : BaseAuditableEntity
{
    public ChatId ChatId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsKicked { get; set; } = false;
    public string Language { get; set; } = "ru";
}
