namespace Application.BotStart.Commands.StartBot;

public class StartBotResult
{
    public static StartBotResult Create(bool isAdmin, bool isSuperAdmin, bool isKicked) => new()
    {
        IsAdmin = isAdmin,
        IsSuperAdmin = isSuperAdmin,
        IsKicked = isKicked
    };

    public bool IsAdmin { get; set; }

    public bool IsSuperAdmin { get; set; }

    public bool IsKicked { get; set; }
}
