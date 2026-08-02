using Domain.Enums;

namespace Web.Models;

public class AppViewModel
{
    public long Id { get; set; }
    public long ClientChatId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Profession { get; set; } = string.Empty;
    public string HasPets { get; set; } = string.Empty;
    public string  Term { get; set; } = string.Empty;
    public string ManagerUsername { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
