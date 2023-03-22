using Domain.Common;

namespace Domain.Entities;

public class Message : BaseAuditableEntity
{
    public string Name { get; set; }

    public string Body { get; set; }

    public string? PathToPhoto { get; set; }
}