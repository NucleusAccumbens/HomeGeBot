using Application.Messages.Interfaces;
using Bot.Common.Abstractions;

namespace Bot.Messages.ClientMessages;

public class ProfessionMessage : BaseMessage
{
    private const string _messageName = "profession";
    
    public ProfessionMessage(IGetMessageQuery getMessageQuery) 
        : base(_messageName, getMessageQuery)
    {
    }

    public override InlineKeyboardMarkup? InlineKeyboardMarkup =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "🔙 back", callbackData: "aGoBack"),
            },
        });
}
