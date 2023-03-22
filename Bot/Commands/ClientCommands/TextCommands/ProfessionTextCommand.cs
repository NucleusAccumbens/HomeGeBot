using Bot.Common.Abstractions;
using Bot.Common.Services;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;

namespace Bot.Commands.ClientCommands.TextCommands;

public class ProfessionTextCommand : BaseTextCommand
{
    private readonly HasPetsMessage _hasPetsMessage;

    private readonly IMemoryCacheService _memoryCacheService;

    public ProfessionTextCommand(HasPetsMessage hasPetsMessage, IMemoryCacheService memoryCacheService)
    {
        _hasPetsMessage = hasPetsMessage;
        _memoryCacheService = memoryCacheService;
    }

    public override string Name => "profession";

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null && update.Message.Text != null)
        {
            long chatId = update.Message.Chat.Id;

            string profession = update.Message.Text;

            try
            {
                int messageId = _memoryCacheService.GetMessageIdFromMemoryCache(chatId);

                var serviceClient = _memoryCacheService.GetClientFromMemoryCache(chatId);

                serviceClient.Profession = profession;

                _memoryCacheService.SetMemoryCache(chatId, serviceClient);

                await MessageService.DeleteMessage(chatId, update.Message.MessageId, client);

                await _hasPetsMessage.EditMessage(chatId, messageId, client,
                    $"<b>Страна:</b> {serviceClient.Country}\n" +
                    $"<b>Деятельность:</b> {serviceClient.Profession}");

                _memoryCacheService.RemoveMessageIdFromMemoryCache(chatId);
                _memoryCacheService.RemoveCommandStateFromMemoryCache(chatId);
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
