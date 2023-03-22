using Bot.Common.Abstractions;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;
using Domain.Entities;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bot.Commands.ClientCommands.CallbackCommands;

public class HasPetsCallbackCommand : BaseCallbackCommand
{
    private readonly ProfessionMessage _professionMessage;

    private readonly TermMessage _termMessage;

    private readonly IMemoryCacheService _memoryCacheService;

    public HasPetsCallbackCommand(ProfessionMessage professionMessage, TermMessage termMessage, IMemoryCacheService memoryCacheService)
    {
        _professionMessage = professionMessage;
        _termMessage = termMessage;
        _memoryCacheService = memoryCacheService;
    }

    public override char CallbackDataCode => 'b';

    public override async Task CallbackExecute(Update update, ITelegramBotClient client)
    {
        if (update.CallbackQuery != null && update.CallbackQuery.Message != null && update.CallbackQuery.Data != null)
        {
            long chatId = update.CallbackQuery.Message.Chat.Id;

            int messageId = update.CallbackQuery.Message.MessageId;

            try
            {
                var serviceClient = _memoryCacheService.GetClientFromMemoryCache(chatId);

                if (update.CallbackQuery.Data == "bGoBack")
                {
                    _memoryCacheService.SetMemoryCache(chatId, "profession");
                    _memoryCacheService.SetMemoryCache(chatId, messageId);
                    
                    await _professionMessage.EditMessage(chatId, messageId, client,
                        $"<b>Страна:</b> {serviceClient.Country}");
                    return;
                }
                
                
                if (update.CallbackQuery.Data == "bДа") serviceClient.HasPets = true;
                if (update.CallbackQuery.Data == "bНет") serviceClient.HasPets = false;

                _memoryCacheService.SetMemoryCache(chatId, serviceClient);

                await _termMessage.EditMessage(chatId, messageId, client,
                    $"<b>Страна:</b> {serviceClient.Country}\n" +
                    $"<b>Деятельность:</b> {serviceClient.Profession}\n" +
                    $"<b>Домашние животные:</b> {update.CallbackQuery.Data[1..]}");
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
