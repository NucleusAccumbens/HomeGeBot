using Bot.Common.Abstractions;
using Bot.Common.Services;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;
using Bot.Session;

namespace Bot.Commands.ClientCommands.CallbackCommands;

public class FlatCallbackCommand : BaseCallbackCommand
{
    private readonly CountryMessage _countryMessage;
    private readonly IBotSessionStore _sessionStore;

    public FlatCallbackCommand(IBotSessionStore sessionStore, CountryMessage countryMessage)
    {
        _sessionStore = sessionStore;
        _countryMessage = countryMessage;
    }

    public override char CallbackDataCode => 'd';

    public override async Task CallbackExecute(Update update, ITelegramBotClient client)
    {
        if (update.CallbackQuery != null && update.CallbackQuery.Message != null)
        {
            long chatId = update.CallbackQuery.Message.Chat.Id;
            int messageId = update.CallbackQuery.Message.MessageId;
            string callbackId = update.CallbackQuery.Id;

            try
            {
                await _sessionStore.ClearAsync(chatId);

                await MessageService.ShowAllert(callbackId, client, "Заявка отменена!");
                await _countryMessage.EditMessage(chatId, messageId, client);
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
