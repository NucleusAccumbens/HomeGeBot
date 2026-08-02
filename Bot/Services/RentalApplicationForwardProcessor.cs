using Application.Messages.Queries.GetMessageBody;
using Application.RentalApplications.Commands.SubmitRentalApplication;
using Application.Users.Queries.GetUserLanguage;
using Bot.Configuration;
using Bot.Session;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Domain.Common;
using MediatR;

namespace Bot.Services;

public class RentalApplicationForwardProcessor : IRentalApplicationForwardProcessor
{
    private readonly IMediator _mediator;
    private readonly IMessageService _messageService;
    private readonly IManagerNotificationFormatter _notificationFormatter;
    private readonly IBotSessionStore _sessionStore;
    private readonly IBotI18n _i18n;
    private readonly BotConfiguration _botConfig;

    public RentalApplicationForwardProcessor(IMediator mediator,
        IMessageService messageService,
        IManagerNotificationFormatter notificationFormatter,
        IBotSessionStore sessionStore,
        IBotI18n i18n,
        IOptions<BotConfiguration> botConfigOptions)
    {
        _mediator = mediator;
        _messageService = messageService;
        _notificationFormatter = notificationFormatter;
        _sessionStore = sessionStore;
        _i18n = i18n;
        _botConfig = botConfigOptions.Value;
    }

    public async Task<string?> ProcessAsync(long chatId, Update update, ITelegramBotClient client, BotSession session, CancellationToken cancellationToken = default)
    {
        if (update.Message == null) return "Сообщение отсутствует.";

        var result = await _mediator.Send(new SubmitRentalApplicationRequest
        {
            ChatId = ChatId.FromLong(chatId),
            Country = session.RentalApplication!.Country!.Value,
            CountryOther = session.RentalApplication.CountryOther,
            Profession = session.RentalApplication.Profession!,
            HasPets = session.RentalApplication.HasPets!.Value,
            Term = session.RentalApplication.Term!.Value,
            TermOther = session.RentalApplication.TermOther
        }, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error;
        }

        var managerChatId = result.Value!.AssignedManagerChatId!.Value.ToLong();

        await client.ForwardMessageAsync(managerChatId, chatId, update.Message.MessageId, cancellationToken: cancellationToken);

        var (managerBody, writeButtonText) = await _notificationFormatter
            .FormatAsync(session.RentalApplication, managerChatId, update.Message.Chat.Username ?? "");

        await _messageService.SendMessage(managerChatId, client, managerBody,
            new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl(text: writeButtonText, url: $"https://t.me/{update.Message.Chat.Username}"),
                },
            }));

        if (session.MessageId.HasValue)
        {
            await _messageService.DeleteMessage(chatId, session.MessageId.Value, client);
        }

        var appBody = await _mediator.Send(new GetMessageBodyQuery("app", await _mediator.Send(new GetUserLanguageQuery(new ChatId(chatId)), cancellationToken)), cancellationToken)
            ?? "Заявка принята! Менеджер скоро свяжется с вами.";
        var appLang = await _mediator.Send(new GetUserLanguageQuery(new ChatId(chatId)), cancellationToken);
        var appKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp(text: _i18n.T("btn.openApplication", appLang), webAppInfo: new WebAppInfo { Url = _botConfig.WebAppUrl }),
            },
        });

        await _messageService.SendMessage(chatId, client, appBody, appKeyboard);
        await _sessionStore.ClearAsync(chatId);

        return null;
    }
}
