using Bot.Common.Abstractions;
using Bot.Common.Services;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;
using Bot.Session;
using Domain.Common;

namespace Bot.Commands.ClientCommands.TextCommands;

public class ProfessionTextCommand : BaseTextCommand
{
    private readonly HasPetsMessage _hasPetsMessage;

    private readonly IBotSessionStore _sessionStore;

    public ProfessionTextCommand(HasPetsMessage hasPetsMessage, IBotSessionStore sessionStore)
    {
        _hasPetsMessage = hasPetsMessage;
        _sessionStore = sessionStore;
    }

    public override string Name => "profession";

    public override BotStep? HandledStep => BotStep.EnterProfession;

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null && update.Message.Text != null)
        {
            long chatId = update.Message.Chat.Id;

            string profession = update.Message.Text;

            try
            {
                var session = await _sessionStore.GetAsync(chatId);
                if (session?.RentalApplication == null || session.MessageId == null)
                    throw new MemoryCacheException();

                session.RentalApplication.Profession = profession;
                session.Step = BotStep.SelectPets;

                await _sessionStore.SaveAsync(session);

                await MessageService.DeleteMessage(chatId, update.Message.MessageId, client);

                await _hasPetsMessage.EditMessage(chatId, session.MessageId.Value, client,
                    $"<b>Страна:</b> {session.RentalApplication.Country?.GetDisplayName()}\n" +
                    $"<b>Деятельность:</b> {session.RentalApplication.Profession}");
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
