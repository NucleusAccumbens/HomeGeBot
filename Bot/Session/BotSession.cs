using Domain.Common;

namespace Bot.Session;

public class BotSession
{
    public ChatId ChatId { get; set; }
    public BotStep Step { get; set; }

    public int? MessageId { get; set; }

    public RentalApplicationDraft? RentalApplication { get; set; }
}
