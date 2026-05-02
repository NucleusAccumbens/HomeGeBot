using Bot.Common.Abstractions;
using Bot.Exceptions;
using Bot.Messages.ClientMessages;
using Bot.Session;
using Domain.Common;
using Domain.Enums;

namespace Bot.Commands.ClientCommands.CallbackCommands;

public class TermCallbackCommand : BaseCallbackCommand
{
    private readonly HasPetsMessage _hasPetsMessage;

    private readonly FlatMessage _flatMessage;

    private readonly IBotSessionStore _sessionStore;

    public TermCallbackCommand(HasPetsMessage hasPetsMessage, FlatMessage flatMessage, IBotSessionStore sessionStore)
    {
        _hasPetsMessage = hasPetsMessage;
        _flatMessage = flatMessage;
        _sessionStore = sessionStore;
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
                var session = await _sessionStore.GetAsync(chatId);
                if (session?.RentalApplication == null)
                    throw new MemoryCacheException();

                if (update.CallbackQuery.Data == "cGoBack")
                {
                    session.Step = BotStep.SelectPets;
                    await _sessionStore.SaveAsync(session);
                    
                    await _hasPetsMessage.EditMessage(chatId, messageId, client,
                        $"<b>Страна:</b> {session.RentalApplication.Country?.GetDisplayName()}\n" +
                        $"<b>Деятельность:</b> {session.RentalApplication.Profession}");
                    return;
                }

                if (update.CallbackQuery.Data == "cSixMonths") session.RentalApplication.Term = Term.SixMonths;
                if (update.CallbackQuery.Data == "cOneYear") session.RentalApplication.Term = Term.OneYear;
                if (update.CallbackQuery.Data == "cOther") session.RentalApplication.Term = Term.Other;

                session.Step = BotStep.WaitForFlatForward;
                session.MessageId = messageId;
                await _sessionStore.SaveAsync(session);
                
                await _flatMessage.EditMessage(chatId, messageId, client,
                    $"<b>Страна:</b> {session.RentalApplication.Country?.GetDisplayName()}\n" +
                    $"<b>Деятельность:</b> {session.RentalApplication.Profession}\n" +
                    $"<b>Домашние животные:</b> {(session.RentalApplication.HasPets == true ? "Да" : "Нет")}\n" +
                    $"<b>Срок аренды:</b> {session.RentalApplication.Term?.GetDisplayName()}");
            }
            catch (MemoryCacheException ex)
            {
                await ex.SendExceptionMessage(chatId, client);
            }
        }
    }
}
