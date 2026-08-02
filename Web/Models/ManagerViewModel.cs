namespace Web.Models;

public class ManagerViewModel
{
    public long ChatId { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public int ApplicationCount { get; set; }
    public bool IsSuperAdmin { get; set; }
}
