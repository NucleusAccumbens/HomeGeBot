using Application.Messages.Interfaces;
using Bot.Common.Abstractions;

namespace Bot.Messages.ClientMessages;

public class CountryMessage : BaseMessage
{
    private const string _messageName = "/start";

    public CountryMessage(IGetMessageQuery getMessageQuery)
        : base(_messageName, getMessageQuery)
    {
    }

    public override InlineKeyboardMarkup? InlineKeyboardMarkup =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Россия", callbackData: "aRussia"),
                InlineKeyboardButton.WithCallbackData(text: "Украина", callbackData: "aUkraine"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Беларусь", callbackData: "aBelarus"),
                InlineKeyboardButton.WithCallbackData(text: "Другое", callbackData: "aOther"),
            },

        });
}
