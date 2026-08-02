using Application.Messages.Queries.GetMessageBody;
using Application.RentalApplications.Commands.SubmitRentalApplication;
using Application.Users.Queries.GetUserLanguage;
using Bot.Common.Abstractions;
using Bot.Services;
using Bot.Configuration;
using Bot.Exceptions;
using Bot.Session;
using Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Bot.Commands.ClientCommands.TextCommands;

public class AppTextCommand : BaseTextCommand
{
    private readonly IBotSessionStore _sessionStore;

    private readonly IMediator _mediator;

    private readonly BotConfiguration _botConfig;

    public AppTextCommand(IBotSessionStore sessionStore,
        IMediator mediator,
        IOptions<BotConfiguration> botConfigOptions)
    {
        _sessionStore = sessionStore;
        _mediator = mediator;
        _botConfig = botConfigOptions.Value;
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
                            var lang = await _mediator.Send(new GetUserLanguageQuery(chatId));
                            var body = await _mediator.Send(new GetMessageBodyQuery("channelError", lang)) ?? "Перешлите пост из канала @propertyintbilisi";

                            await MessageService.SendMessage(chatId, client, body, null);

                            return;
                        }
                    }

                    if (update.Message.ForwardFromChat.Id == _botConfig.SourceChannelId)
                    {
                        if (hasContent)
                        {                          
                            var session = await _sessionStore.GetAsync(chatId);
                            if (session?.RentalApplication == null)
                                throw new SessionExpiredException();

                            var result = await _mediator.Send(new SubmitRentalApplicationRequest
                            {
                                ChatId = chatId,
                                Country = session.RentalApplication.Country!.Value,
                                CountryOther = session.RentalApplication.CountryOther,
                                Profession = session.RentalApplication.Profession!,
                                HasPets = session.RentalApplication.HasPets!.Value,
                                Term = session.RentalApplication.Term!.Value,
                                TermOther = session.RentalApplication.TermOther
                            });

                            if (result.IsFailure)
                            {
                                await MessageService.SendMessage(chatId, client, result.Error!, null);
                                return;
                            }

                            await client.ForwardMessageAsync((long)result.Value!.AssignedManagerChatId!.Value, (long)chatId, update.Message.MessageId);

                            var managerChatId = (long)result.Value.AssignedManagerChatId!.Value;
                            var managerLang = await _mediator.Send(new GetUserLanguageQuery(managerChatId));
                            var managerBody = await _mediator.Send(new GetMessageBodyQuery("managerNotification", managerLang))
                                ?? "<b>Страна:</b> {country}\n<b>Деятельность:</b> {profession}\n<b>Домашние животные:</b> {pets}\n<b>Срок аренды:</b> {term}";

                            var countryDisplay = session.RentalApplication.Country == Domain.Enums.Country.Other && !string.IsNullOrWhiteSpace(session.RentalApplication.CountryOther)
                                ? MessageService.Escape(session.RentalApplication.CountryOther) : session.RentalApplication.Country?.GetDisplayName();
                            var termDisplay = session.RentalApplication.Term == Domain.Enums.Term.Other && !string.IsNullOrWhiteSpace(session.RentalApplication.TermOther)
                                ? MessageService.Escape(session.RentalApplication.TermOther) : session.RentalApplication.Term?.GetDisplayName();
                            var petsDisplay = session.RentalApplication.HasPets.Value
                                ? (managerLang == "en" ? "Yes" : managerLang == "ka" ? "დიახ" : "Да")
                                : (managerLang == "en" ? "No" : managerLang == "ka" ? "არა" : "Нет");

                            managerBody = managerBody
                                .Replace("{country}", countryDisplay ?? "—")
                                .Replace("{profession}", MessageService.Escape(session.RentalApplication.Profession) ?? "—")
                                .Replace("{pets}", petsDisplay)
                                .Replace("{term}", termDisplay ?? "—");

                            await MessageService.SendMessage(managerChatId, client, managerBody,
                                new(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithUrl(text: BotI18n.T("btn.write", managerLang), url: $"https://t.me/{update.Message.Chat.Username}"),
                                    },
                                }));

                            if (session.MessageId.HasValue)
                            {
                                await MessageService.DeleteMessage(chatId, session.MessageId.Value, client);
                            }
                            
                            var appLang = await _mediator.Send(new GetUserLanguageQuery(chatId));
                            var appBody = await _mediator.Send(new GetMessageBodyQuery("app", appLang)) ?? "Заявка принята! Менеджер скоро свяжется с вами.";
                            
                            var appKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithWebApp(text: BotI18n.T("btn.openApplication", appLang), webAppInfo: new WebAppInfo { Url = _botConfig.WebAppUrl }),
                                },
                            });
                            
                            await MessageService.SendMessage(chatId, client, appBody, appKeyboard);

                            await _sessionStore.ClearAsync(chatId);

                            return;
                        }
                    }
                }
                else
                {
                    var errLang = await _mediator.Send(new GetUserLanguageQuery(chatId));
                    var channelErrBody = await _mediator.Send(new GetMessageBodyQuery("channelError", errLang)) ?? "Перешлите пост из канала @propertyintbilisi";

                    await MessageService.SendMessage(chatId, client, channelErrBody, null);
                }
            }
            catch (SessionExpiredException)
            {
                await MessageService.SendMessage(chatId, client, SessionExpiredException.MessageText, null);
            }
            catch (ValidationException ex)
            {
                var msg = string.Join("\n", ex.Errors.Select(e => e.ErrorMessage));
                await MessageService.SendMessage(chatId, client, msg, null);
            }
        }
    }
}
