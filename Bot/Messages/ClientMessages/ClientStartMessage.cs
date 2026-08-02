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
    private readonly IBotI18n _i18n;

    public ClientStartMessage(IMediator mediator, IOptions<BotConfiguration> botConfig, IMessageService messageService, IBotI18n i18n)
        : base(_messageName, mediator, messageService)
    {
        _botConfig = botConfig.Value;
        _i18n = i18n;
    }

    public override InlineKeyboardMarkup? GetInlineKeyboardMarkup(string language) =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp(text: _i18n.T("btn.openApplication", language), webAppInfo: new WebAppInfo { Url = _botConfig.WebAppUrl }),
            },
        });
}
