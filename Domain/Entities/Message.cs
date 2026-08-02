using Domain.Common;

namespace Domain.Entities;

public class Message : BaseAuditableEntity
{
    public Message(string name, string body, string? bodyEn = null, string? bodyKa = null, string? pathToPhoto = null)
    {
        Name = name;
        Body = body;
        BodyEn = bodyEn;
        BodyKa = bodyKa;
        PathToPhoto = pathToPhoto;
    }

    private Message() { }

    public string Name { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? BodyEn { get; private set; }
    public string? BodyKa { get; private set; }
    public string? PathToPhoto { get; private set; }

    public void UpdateBody(string body) => Body = body;

    public void UpdatePhotoPath(string? pathToPhoto) => PathToPhoto = pathToPhoto;
}
