using Domain.Common;

namespace Domain.Entities;

public class Admin : BaseAuditableEntity
{   
    public long ChatId { get; set; }
    public List<Client> Clients { get; set; } = new();
}
