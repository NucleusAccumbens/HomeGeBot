namespace Bot.Configuration;

public class BotConfiguration
{
    public long SourceChannelId { get; set; }
    public long[] AdminWhitelist { get; set; } = Array.Empty<long>();
}
