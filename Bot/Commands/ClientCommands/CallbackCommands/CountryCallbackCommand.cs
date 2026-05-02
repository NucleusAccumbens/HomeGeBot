using Application.TlgUsers.Interfaces;
using Bot.Common.Abstractions;
using Bot.Common.Services;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;
using Bot.Session;
using Domain.Common;
using Domain.Enums;

namespace Bot.Commands.ClientCommands.CallbackCommands;

public class CountryCallbackCommand : BaseCallbackCommand
{
    private readonly ProfessionMessage _professionMessage;

    private readonly CountryMessage _countryMessage;

    private readonly IBotSessionStore _sessionStore;

    private readonly IUpdateTlgUserCommand _updateTlgUserCommand;

    public CountryCallbackCommand(ProfessionMessage professionMessage, CountryMessage countryMessage,
        IBotSessionStore sessionStore, IUpdateTlgUserCommand updateTlgUserCommand)
    {
        _countryMessage = countryMessage;
        _professionMessage = professionMessage;
        _sessionStore = sessionStore;
        _updateTlgUserCommand = updateTlgUserCommand;
    }
    
    public override char CallbackDataCode => 'a';

    public override async Task CallbackExecute(Update update, ITelegramBotClient client)
    {
        if (update.CallbackQuery != null && update.CallbackQuery.Message != null)
        {
            long chatId = update.CallbackQuery.Message.Chat.Id;

            int messageId = update.CallbackQuery.Message.MessageId;

            string? username = update.CallbackQuery.Message.Chat.Username;

            if (username == null)
            {
                await MessageService.ShowAllert(update.CallbackQuery.Id, client,
                    "В вашем телеграм профиле отсутствует username (имя пользователя).\n\n" +
                    "Чтобы оставить заявку, добавьте username в свой профиль. ");
                return;
            }

            await _updateTlgUserCommand.UpdateUsernameAsync(chatId, username);

            try
            {
                if (update.CallbackQuery.Data == "aGoBack")
                {
                    await _sessionStore.ClearAsync(chatId);
                    await _countryMessage.EditMessage(chatId, messageId, client);
                    return;
                }
                
                var session = await _sessionStore.GetAsync(chatId) ?? new BotSession { ChatId = chatId };
                session.RentalApplication = new RentalApplicationDraft();

                if (update.CallbackQuery.Data == "aRussia") session.RentalApplication.Country = Country.Russia;
                if (update.CallbackQuery.Data == "aUkraine") session.RentalApplication.Country = Country.Ukraine;
                if (update.CallbackQuery.Data == "aBelarus") session.RentalApplication.Country = Country.Belarus;
                if (update.CallbackQuery.Data == "aOther") session.RentalApplication.Country = Country.Other;

                session.Step = BotStep.EnterProfession;
                session.MessageId = messageId;

                await _sessionStore.SaveAsync(session);

                await _professionMessage.EditMessage(chatId, messageId, client,
                    $"<b>Страна:</b> {session.RentalApplication.Country?.GetDisplayName()}");
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
