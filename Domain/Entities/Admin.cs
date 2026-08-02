using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Admin : BaseAuditableEntity
{
    public ChatId ChatId { get; set; }
    public List<Client> Clients { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public AdminRole Role { get; set; } = AdminRole.Admin;
}
