using Bot.Session;

namespace Bot.Common.Abstractions;

public abstract class BaseTextCommand
{
    public abstract string Name { get; }

    public virtual BotStep? HandledStep { get; } = null;

    public abstract Task Execute(Update update, ITelegramBotClient client);
}
