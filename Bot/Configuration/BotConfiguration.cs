using Domain.Common;

namespace Bot.Configuration;

public class BotConfiguration
{
    public long SourceChannelId { get; set; }
    public ChatId[] AdminWhitelist { get; set; } = Array.Empty<ChatId>();
    public string AdminPanelUrl { get; set; } = "";
    public string WebAppUrl { get; set; } = "";
}
