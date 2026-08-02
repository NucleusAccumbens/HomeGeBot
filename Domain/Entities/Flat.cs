using Domain.Common;

namespace Domain.Entities;

public class Flat : BaseAuditableEntity
{
    public Flat(string? itemId, string? link, string? ownerNumber, string? comment)
    {
        ItemId = itemId;
        Link = link;
        OwnerNumber = ownerNumber;
        Comment = comment;
    }

    private Flat() { }

    public string? ItemId { get; private set; }
    public string? Link { get; private set; }
    public string? OwnerNumber { get; private set; }
    public string? Comment { get; private set; }

    public void UpdateComment(string? comment) => Comment = comment;

    public void UpdateDetails(string? link, string? ownerNumber)
    {
        Link = link;
        OwnerNumber = ownerNumber;
    }
}
