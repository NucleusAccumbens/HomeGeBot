namespace Bot.Configuration;

public class BotConfiguration
{
    public long SourceChannelId { get; set; }
    public long[] AdminWhitelist { get; set; } = Array.Empty<long>();
    public string AdminPanelUrl { get; set; } = "";
    public string WebAppUrl { get; set; } = "";
}
