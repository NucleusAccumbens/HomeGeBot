namespace Application.AdminManagement;

public class BotUserDto
{
    public long ChatId { get; set; }

    public string? Username { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsKicked { get; set; }

    public DateTime CreatedAt { get; set; }
}
