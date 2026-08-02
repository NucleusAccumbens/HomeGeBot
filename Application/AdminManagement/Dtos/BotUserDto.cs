using Domain.Common;

namespace Application.AdminManagement.Dtos;

public class BotUserDto
{
    public ChatId ChatId { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsKicked { get; set; }

    public DateTime CreatedAt { get; set; }
}
