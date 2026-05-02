namespace Bot.Session;

public class BotSession
{
    public long ChatId { get; set; }

    public BotStep Step { get; set; }

    public int? MessageId { get; set; }

    public RentalApplicationDraft? RentalApplication { get; set; }
}
