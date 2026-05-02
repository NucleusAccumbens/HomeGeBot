using Bot.Common.Abstractions;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;
using Bot.Session;
using Domain.Common;

namespace Bot.Commands.ClientCommands.CallbackCommands;

public class HasPetsCallbackCommand : BaseCallbackCommand
{
    private readonly ProfessionMessage _professionMessage;

    private readonly TermMessage _termMessage;

    private readonly IBotSessionStore _sessionStore;

    public HasPetsCallbackCommand(ProfessionMessage professionMessage, TermMessage termMessage, IBotSessionStore sessionStore)
    {
        _professionMessage = professionMessage;
        _termMessage = termMessage;
        _sessionStore = sessionStore;
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
                var session = await _sessionStore.GetAsync(chatId);
                if (session?.RentalApplication == null)
                    throw new MemoryCacheException();

                if (update.CallbackQuery.Data == "bGoBack")
                {
                    session.Step = BotStep.EnterProfession;
                    session.MessageId = messageId;
                    await _sessionStore.SaveAsync(session);
                    
                    await _professionMessage.EditMessage(chatId, messageId, client,
                        $"<b>Страна:</b> {session.RentalApplication.Country?.GetDisplayName()}");
                    return;
                }
                
                if (update.CallbackQuery.Data == "bДа") session.RentalApplication.HasPets = true;
                if (update.CallbackQuery.Data == "bНет") session.RentalApplication.HasPets = false;

                session.Step = BotStep.SelectTerm;
                await _sessionStore.SaveAsync(session);

                await _termMessage.EditMessage(chatId, messageId, client,
                    $"<b>Страна:</b> {session.RentalApplication.Country?.GetDisplayName()}\n" +
                    $"<b>Деятельность:</b> {session.RentalApplication.Profession}\n" +
                    $"<b>Домашние животные:</b> {(session.RentalApplication.HasPets == true ? "Да" : "Нет")}");
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
