using Bot.Common.Abstractions;
using Bot.Configuration;
using Bot.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace Bot.Messages.ClientMessages;

public class ClientStartMessage : BaseMessage
{
    private const string _messageName = "start";
    private readonly BotConfiguration _botConfig;

    public ClientStartMessage(IMediator mediator, IOptions<BotConfiguration> botConfig)
        : base(_messageName, mediator)
    {
        _botConfig = botConfig.Value;
    }

    public override InlineKeyboardMarkup? GetInlineKeyboardMarkup(string language) =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp(text: BotI18n.T("btn.openApplication", language), webAppInfo: new WebAppInfo { Url = _botConfig.WebAppUrl }),
            },
        });
}
