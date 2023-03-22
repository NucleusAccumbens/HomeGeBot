using Microsoft.AspNetCore.Mvc;

namespace Web.Models;

public class FlatViewModel
{
    public string ItemId { get; set; }

    public string PublicationDate { get; set; }

    public string Link { get; set; }

    public string OwnerNumber { get; set; }

    public string? Comment { get; set; }
}
