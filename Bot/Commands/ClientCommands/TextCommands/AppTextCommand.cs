using Application.Messages.Queries.GetMessageBody;
using Application.RentalApplications.Commands.SubmitRentalApplication;
using Application.Users.Queries.GetUserLanguage;
using Bot.Common.Abstractions;
using Bot.Services;
using Bot.Configuration;
using Bot.Exceptions;
using Bot.Session;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Bot.Commands.ClientCommands.TextCommands;

public class AppTextCommand : BaseTextCommand
{
    private readonly IBotSessionStore _sessionStore;
    private readonly IMediator _mediator;
    private readonly BotConfiguration _botConfig;
    private readonly IMessageService _messageService;
    private readonly IBotI18n _i18n;
    private readonly IManagerNotificationFormatter _notificationFormatter;

    public AppTextCommand(IBotSessionStore sessionStore,
        IMediator mediator,
        IOptions<BotConfiguration> botConfigOptions,
        IMessageService messageService,
        IBotI18n i18n,
        IManagerNotificationFormatter notificationFormatter)
    {
        _sessionStore = sessionStore;
        _mediator = mediator;
        _botConfig = botConfigOptions.Value;
        _messageService = messageService;
        _i18n = i18n;
        _notificationFormatter = notificationFormatter;
    }

    public override string Name => "app";

    public override BotStep? HandledStep => BotStep.WaitForFlatForward;

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            try
            {
                if (update.Message.ForwardFromChat != null)
                {
                    var hasContent = update.Message.Caption != null || update.Message.Text != null;

                    if (update.Message.ForwardFromChat.Id != _botConfig.SourceChannelId)
                    {
                        if (hasContent)
                        {
                            var body = await GetLocalizedMessage(chatId, "channelError", "Перешлите пост из канала @propertyintbilisi");
                            await _messageService.SendMessage(chatId, client, body, null);
                            return;
                        }
                    }

                    if (update.Message.ForwardFromChat.Id == _botConfig.SourceChannelId)
                    {
                        if (hasContent)
                        {
                            await ProcessApplicationForward(chatId, update, client);
                            return;
                        }
                    }
                }
                else
                {
                    var body = await GetLocalizedMessage(chatId, "channelError", "Перешлите пост из канала @propertyintbilisi");
                    await _messageService.SendMessage(chatId, client, body, null);
                }
            }
            catch (SessionExpiredException)
            {
                await _messageService.SendMessage(chatId, client, SessionExpiredException.MessageText, null);
            }
            catch (ValidationException ex)
            {
                var msg = string.Join("\n", ex.Errors.Select(e => e.ErrorMessage));
                await _messageService.SendMessage(chatId, client, msg, null);
            }
        }
    }

    private async Task ProcessApplicationForward(long chatId, Update update, ITelegramBotClient client)
    {
        if (update.Message == null) return;
        var session = await _sessionStore.GetAsync(chatId);
        if (session?.RentalApplication == null)
            throw new SessionExpiredException();

        var result = await _mediator.Send(new SubmitRentalApplicationRequest
        {
            ChatId = ChatId.FromLong(chatId),
            Country = session.RentalApplication.Country!.Value,
            CountryOther = session.RentalApplication.CountryOther,
            Profession = session.RentalApplication.Profession!,
            HasPets = session.RentalApplication.HasPets!.Value,
            Term = session.RentalApplication.Term!.Value,
            TermOther = session.RentalApplication.TermOther
        });

        if (result.IsFailure)
        {
            await _messageService.SendMessage(chatId, client, result.Error!, null);
            return;
        }

        var managerChatId = (long)result.Value!.AssignedManagerChatId!.Value;

        await client.ForwardMessageAsync(managerChatId, chatId, update.Message.MessageId);

        var (managerBody, writeButtonText) = await _notificationFormatter
            .FormatAsync(session.RentalApplication, managerChatId, update.Message.Chat.Username ?? "");

        await _messageService.SendMessage(managerChatId, client, managerBody,
            new(new[]
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

        var appBody = await GetLocalizedMessage(chatId, "app", "Заявка принята! Менеджер скоро свяжется с вами.");
        var appLang = await _mediator.Send(new GetUserLanguageQuery(chatId));
        var appKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp(text: _i18n.T("btn.openApplication", appLang), webAppInfo: new WebAppInfo { Url = _botConfig.WebAppUrl }),
            },
        });

        await _messageService.SendMessage(chatId, client, appBody, appKeyboard);
        await _sessionStore.ClearAsync(chatId);
    }

    private async Task<string> GetLocalizedMessage(long chatId, string messageName, string fallback)
    {
        var lang = await _mediator.Send(new GetUserLanguageQuery(chatId));
        return await _mediator.Send(new GetMessageBodyQuery(messageName, lang)) ?? fallback;
    }
}
