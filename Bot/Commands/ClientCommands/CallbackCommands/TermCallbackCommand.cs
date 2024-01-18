using Bot.Common.Abstractions;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;
using Domain.Entities;
using Domain.Enums;

namespace Bot.Commands.ClientCommands.CallbackCommands;

public class TermCallbackCommand : BaseCallbackCommand
{
    private readonly HasPetsMessage _hasPetsMessage;

    private readonly FlatMessage _flatMessage;

    private readonly IMemoryCacheService _memoryCacheService;

    public TermCallbackCommand(HasPetsMessage hasPetsMessage, FlatMessage flatMessage, IMemoryCacheService memoryCacheService)
    {
        _hasPetsMessage = hasPetsMessage;
        _flatMessage = flatMessage;
        _memoryCacheService = memoryCacheService;
    }

    public override char CallbackDataCode => 'c';

    public override async Task CallbackExecute(Update update, ITelegramBotClient client)
    {
        if (update.CallbackQuery != null && update.CallbackQuery.Message != null && update.CallbackQuery.Data != null)
        {
            long chatId = update.CallbackQuery.Message.Chat.Id;

            int messageId = update.CallbackQuery.Message.MessageId;

            try
            {
                var serviceClient = _memoryCacheService.GetClientFromMemoryCache(chatId);

                if (update.CallbackQuery.Data == "cGoBack")
                {
                    await _hasPetsMessage.EditMessage(chatId, messageId, client,
                        $"<b>Страна:</b> {serviceClient.Country}\n" +
                        $"<b>Деятельность:</b> {serviceClient.Profession}");
                    return;
                }


                if (update.CallbackQuery.Data == "cПолгода") serviceClient.Term = Term.Полгода;
                if (update.CallbackQuery.Data == "cГод") serviceClient.Term = Term.Год;
                if (update.CallbackQuery.Data == "cДругое") serviceClient.Term = Term.Другое;

                _memoryCacheService.SetMemoryCache(chatId, serviceClient);
                _memoryCacheService.SetMemoryCache(chatId, messageId);
                _memoryCacheService.SetMemoryCache(chatId, "app");
                
                await _flatMessage.EditMessage(chatId, messageId, client,
                    $"<b>Страна:</b> {serviceClient.Country}\n" +
                    $"<b>Деятельность:</b> {serviceClient.Profession}\n" +
                    $"<b>Домашние животные:</b> {GetHasPetsStringValue(serviceClient)}\n" +
                    $"<b>Срок аренды:</b> {serviceClient.Term}");
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }

    private static string GetHasPetsStringValue(Client client)
    {
        if (client.HasPets == true) return "Да";
        else return "Нет";
    }
}
