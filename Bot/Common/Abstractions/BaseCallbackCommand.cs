namespace Bot.Common.Abstractions;

public abstract class BaseCallbackCommand
{
    public abstract char CallbackDataCode { get; }

    public abstract Task CallbackExecute(Update update, ITelegramBotClient client);

    public virtual bool Contains(CallbackQuery callbackQuery)
    {
        if (callbackQuery == null || callbackQuery.Data == null)
            return false;

        char code = callbackQuery.Data.ToString().FirstOrDefault();

        if (code != CallbackDataCode)
            return false;

        return callbackQuery.Data.Contains(CallbackDataCode);
    }
}
