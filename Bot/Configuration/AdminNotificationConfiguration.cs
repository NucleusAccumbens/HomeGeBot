using Domain.Common;

namespace Bot.Configuration;

public class AdminNotificationConfiguration
{
    public ChatId[] ChatIds { get; set; } = Array.Empty<ChatId>();
}
