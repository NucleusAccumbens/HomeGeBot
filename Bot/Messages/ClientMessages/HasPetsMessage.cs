using Application.Messages.Interfaces;
using Bot.Common.Abstractions;

namespace Bot.Messages.ClientMessages;

public class HasPetsMessage : BaseMessage
{
    private const string _messageName = "hasPets";
    
    public HasPetsMessage(IGetMessageQuery getMessageQuery) 
        : base(_messageName, getMessageQuery)
    {
    }

    public override InlineKeyboardMarkup? InlineKeyboardMarkup =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Да", callbackData: "bДа"),
                InlineKeyboardButton.WithCallbackData(text: "Нет", callbackData: "bНет"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "🔙 back", callbackData: "bGoBack"),
            },
        });
}
