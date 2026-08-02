namespace Bot.Common.Abstractions;

public abstract class BaseCallbackCommand
{
    public abstract char CallbackDataCode { get; }

    public abstract Task CallbackExecute(Update update, ITelegramBotClient client);

    public virtual bool Contains(CallbackQuery callbackQuery)
    {
        return callbackQuery?.Data != null && callbackQuery.Data.FirstOrDefault() == CallbackDataCode;
    }
}
