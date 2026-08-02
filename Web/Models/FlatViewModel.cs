using Microsoft.AspNetCore.Mvc;

namespace Web.Models;

public class FlatViewModel
{
    public string ItemId { get; set; } = string.Empty;

    public string PublicationDate { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    public string OwnerNumber { get; set; } = string.Empty;

    public string? Comment { get; set; }
}
