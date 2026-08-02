using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Admin : BaseAuditableEntity
{
    public ChatId ChatId { get; set; }
    public List<Client> Clients { get; private set; } = new();
    public bool IsActive { get; private set; } = true;
    public AdminRole Role { get; set; } = AdminRole.Admin;

    public void AssignClient(Client client) => Clients.Add(client);

    public bool RemoveClient(Client client) => Clients.Remove(client);

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
