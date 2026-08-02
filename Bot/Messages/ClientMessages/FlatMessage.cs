using Bot.Common.Abstractions;
using Bot.Services;
using MediatR;

namespace Bot.Messages.ClientMessages;

public class FlatMessage : BaseMessage
{
    private const string _messageName = "sendFlat";
    private readonly IBotI18n _i18n;

    public FlatMessage(IMediator mediator, IMessageService messageService, IBotI18n i18n)
        : base(_messageName, mediator, messageService)
    {
        _i18n = i18n;
    }

    public override InlineKeyboardMarkup? GetInlineKeyboardMarkup(string language) =>
       new(new[]
       {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: _i18n.T("btn.cancelApplication", language), callbackData: "dCancel"),
            },
       });
}
