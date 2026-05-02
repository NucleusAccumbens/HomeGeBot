using Application.Messages.Interfaces;
using Application.RentalApplications;
using Bot.Common.Abstractions;
using Bot.Common.Services;
using Bot.Configuration;
using Bot.Exceptions;
using Bot.Session;
using Domain.Common;
using Microsoft.Extensions.Options;

namespace Bot.Commands.ClientCommands.TextCommands;

public class AppTextCommand : BaseTextCommand
{
    private readonly IBotSessionStore _sessionStore;

    private readonly ISubmitRentalApplicationUseCase _submitRentalApplicationUseCase;

    private readonly IGetMessageQuery _getMessageQuery;

    private readonly BotConfiguration _botConfig;

    public AppTextCommand(IBotSessionStore sessionStore, 
        ISubmitRentalApplicationUseCase submitRentalApplicationUseCase, 
        IGetMessageQuery getMessageQuery,
        IOptions<BotConfiguration> botConfigOptions)
    {
        _sessionStore = sessionStore;
        _submitRentalApplicationUseCase = submitRentalApplicationUseCase;
        _getMessageQuery = getMessageQuery;
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
                    if (update.Message.ForwardFromChat.Id != _botConfig.SourceChannelId)
                    {
                        if (update.Message.Caption != null)
                        {
                            var message = await _getMessageQuery.GetMessageAsync("channelError");
                            var body = message?.Body ?? "Перешлите пост из канала @propertyintbilisi";

                            await MessageService.SendMessage(chatId, client, body, null);

                            return;
                        }
                    }

                    if (update.Message.ForwardFromChat.Id == _botConfig.SourceChannelId)
                    {                       
                        if (update.Message.Caption != null)
                        {                          
                            var session = await _sessionStore.GetAsync(chatId);
                            if (session?.RentalApplication == null || session.MessageId == null)
                                throw new MemoryCacheException();

                            var message = await _getMessageQuery.GetMessageAsync("app");

                            var result = await _submitRentalApplicationUseCase.ExecuteAsync(new SubmitRentalApplicationRequest
                            {
                                ChatId = chatId,
                                Country = session.RentalApplication.Country!.Value,
                                Profession = session.RentalApplication.Profession!,
                                HasPets = session.RentalApplication.HasPets!.Value,
                                Term = session.RentalApplication.Term!.Value
                            });

                            if (!result.Success)
                            {
                                await MessageService.SendMessage(chatId, client, result.ErrorMessage!, null);
                                return;
                            }

                            await client.ForwardMessageAsync(result.AssignedManagerChatId!.Value, chatId, update.Message.MessageId);
                            await MessageService.SendMessage(result.AssignedManagerChatId.Value, client,
                                $"<b>Страна:</b> {session.RentalApplication.Country?.GetDisplayName()}\n" +
                                $"<b>Деятельность:</b> {session.RentalApplication.Profession}\n" +
                                $"<b>Домашние животные:</b> {(session.RentalApplication.HasPets.Value ? "Да" : "Нет")}\n" +
                                $"<b>Срок аренды:</b> {session.RentalApplication.Term?.GetDisplayName()}",
                                new(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithUrl(text: "Написать", url: $"http://t.me/{update.Message.Chat.Username}"),
                                    },
                                }));

                            await MessageService.DeleteMessage(chatId, session.MessageId.Value, client);
                            var appBody = message?.Body ?? "Заявка принята! Менеджер скоро свяжется с вами.";
                            await MessageService.SendMessage(chatId, client, appBody, null);

                            await _sessionStore.ClearAsync(chatId);

                            return;
                        }

                        return;
                    }
                }
                else
                {
                    if (update.Message.Caption != null)
                    {
                        var channelErrMsg = await _getMessageQuery.GetMessageAsync("channelError");
                        var channelErrBody = channelErrMsg?.Body ?? "Перешлите пост из канала @propertyintbilisi";

                        await MessageService.SendMessage(chatId, client, channelErrBody, null);

                        return;
                    }
                }
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
