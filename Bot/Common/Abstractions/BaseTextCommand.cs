namespace Bot.Common.Abstractions;

public abstract class BaseTextCommand
{
    public abstract string Name { get; }

    public abstract Task Execute(Update update, ITelegramBotClient client);

    public virtual bool? Contains(Message message)
    {
        if (message.Type != MessageType.Text)
            return false;

        return message.Text?.Contains(Name);
    }
}
