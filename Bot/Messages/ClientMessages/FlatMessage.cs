using Application.Messages.Interfaces;
using Bot.Common.Abstractions;

namespace Bot.Messages.ClientMessages;

public class FlatMessage : BaseMessage
{
    private const string _messageName = "sendFlat";
    
    public FlatMessage(IGetMessageQuery getMessageQuery) 
        : base(_messageName, getMessageQuery)
    {
    }

    public override InlineKeyboardMarkup? InlineKeyboardMarkup =>
       new(new[]
       {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "✖️ Отменить заявку", callbackData: "dCancel"),
            },
       });
}
