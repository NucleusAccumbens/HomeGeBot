using Application.Messages.Interfaces;
using Bot.Common.Abstractions;

namespace Bot.Messages.ClientMessages;

public class TermMessage : BaseMessage
{
    private const string _messageName = "term";
    
    public TermMessage(IGetMessageQuery getMessageQuery) 
        : base(_messageName, getMessageQuery)
    {
    }

    public override InlineKeyboardMarkup? InlineKeyboardMarkup =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "6 месяцев", callbackData: "cSixMonths"),
                InlineKeyboardButton.WithCallbackData(text: "1 год", callbackData: "cOneYear"),
                InlineKeyboardButton.WithCallbackData(text: "Другое", callbackData: "cOther"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "🔙 back", callbackData: "cGoBack"),
            },
        });
}
