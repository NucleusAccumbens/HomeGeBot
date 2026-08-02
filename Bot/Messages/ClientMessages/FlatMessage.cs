using Bot.Common.Abstractions;
using Bot.Services;
using MediatR;

namespace Bot.Messages.ClientMessages;

public class FlatMessage : BaseMessage
{
    private const string _messageName = "sendFlat";

    public FlatMessage(IMediator mediator)
        : base(_messageName, mediator)
    {
    }

    public override InlineKeyboardMarkup? GetInlineKeyboardMarkup(string language) =>
       new(new[]
       {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: BotI18n.T("btn.cancelApplication", language), callbackData: "dCancel"),
            },
       });
}
