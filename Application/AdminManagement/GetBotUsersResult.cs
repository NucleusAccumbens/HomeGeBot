namespace Application.AdminManagement;

public class GetBotUsersResult
{
    public bool Success { get; set; }

    public List<BotUserDto> Users { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
