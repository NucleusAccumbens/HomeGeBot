using Application.TlgUsers.Interfaces;
using Bot.Common.Abstractions;
using Bot.Common.Services;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;
using Domain.Entities;
using Domain.Enums;

namespace Bot.Commands.ClientCommands.CallbackCommands;

public class CountryCallbackCommand : BaseCallbackCommand
{
    private readonly ProfessionMessage _professionMessage;

    private readonly CountryMessage _countryMessage;

    private readonly IMemoryCacheService _memoryCacheService;

    private readonly IUpdateTlgUserCommand _updateTlgUserCommand;

    public CountryCallbackCommand(ProfessionMessage professionMessage, CountryMessage countryMessage,
        IMemoryCacheService memoryCacheService, IUpdateTlgUserCommand updateTlgUserCommand)
    {
        _countryMessage = countryMessage;
        _professionMessage = professionMessage;
        _memoryCacheService = memoryCacheService;
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
                    _memoryCacheService.RemoveCommandStateFromMemoryCache(chatId);

                    _memoryCacheService.RemoveClienteFromMemoryCache(messageId);
                    
                    await _countryMessage.EditMessage(chatId, messageId, client);

                    return;
                }
                
                var serviceClient = new Client() { ChatId = chatId };

                if (update.CallbackQuery.Data == "aРоссия") serviceClient.Country = Country.Россия;
                if (update.CallbackQuery.Data == "aУкраина") serviceClient.Country = Country.Украина;
                if (update.CallbackQuery.Data == "aБеларусь") serviceClient.Country = Country.Беларусь;
                if (update.CallbackQuery.Data == "aДругое") serviceClient.Country = Country.Другое;

                _memoryCacheService.SetMemoryCache(chatId, serviceClient);
                _memoryCacheService.SetMemoryCache(chatId, "profession");
                _memoryCacheService.SetMemoryCache(chatId, messageId);

                await _professionMessage.EditMessage(chatId, messageId, client, 
                    $"<b>Страна:</b> {serviceClient.Country}");
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
