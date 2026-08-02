using Bot.Common.Abstractions;
using Bot.Configuration;
using Bot.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace Bot.Messages.GeneralMessages;

public class AdminStartMessage : BaseMessage
{
    private const string _messageName = "adminStartMessage";
    private readonly BotConfiguration _botConfig;

    public AdminStartMessage(IMediator mediator, IOptions<BotConfiguration> botConfig)
        : base(_messageName, mediator)
    {
        _botConfig = botConfig.Value;
    }

    public override InlineKeyboardMarkup? GetInlineKeyboardMarkup(string language) =>
        new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp(text: BotI18n.T("btn.openApp", language), webAppInfo: new WebAppInfo { Url = _botConfig.AdminPanelUrl }),
            },
        });
}
