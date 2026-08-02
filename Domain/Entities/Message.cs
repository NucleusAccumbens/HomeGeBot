using Domain.Common;

namespace Domain.Entities;

public class Message : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? BodyEn { get; set; }

    public string? BodyKa { get; set; }

    public string? PathToPhoto { get; set; }
}