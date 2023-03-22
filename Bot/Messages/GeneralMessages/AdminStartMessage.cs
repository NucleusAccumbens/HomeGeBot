using Application.Messages.Interfaces;
using Bot.Common.Abstractions;

namespace Bot.Messages.GeneralMessages;

public class AdminStartMessage : BaseMessage
{
    private const string _messageName = "adminStartMessage";
    
    public AdminStartMessage(IGetMessageQuery getMessageQuery) 
        : base(_messageName, getMessageQuery)
    {
    }

    public override InlineKeyboardMarkup? InlineKeyboardMarkup =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithUrl(text: "Перейти в админку", url: "https://propertyintbilisi-bot.herokuapp.com"),
            },
           
        });
}
