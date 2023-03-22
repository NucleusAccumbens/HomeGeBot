using Application.Admins.Interfaces;
using Application.TlgUsers.Interfaces;
using Bot.Common.Abstractions;
using Bot.Common.Services;

namespace Bot.Commands.GeneralCommands.TextCommands;

public class AddAdminTextCommand : BaseTextCommand
{
    private readonly ICreateAdminCommand _createAdminCommand;

    private readonly IUpdateTlgUserCommand _updateTlgUserCommand;

    public AddAdminTextCommand(ICreateAdminCommand createAdminCommand, 
        IUpdateTlgUserCommand updateTlgUserCommand)
    {
        _createAdminCommand = createAdminCommand;
        _updateTlgUserCommand = updateTlgUserCommand;
    }

    public override string Name => "/addAdmin";

    public override async Task Execute(Update update, ITelegramBotClient client)
    {
        if (update.Message != null)
        {
            long chatId = update.Message.Chat.Id;

            await _updateTlgUserCommand.UpdateTlgUserIsAdminAsync(chatId, true);

            await _createAdminCommand.CreateAdminAsync(
                new Domain.Entities.Admin()
                {
                    ChatId = chatId,
                    Clients = new(),
                    CreatedAt = DateTime.UtcNow,
                });

            await MessageService
                .SendMessage(chatId, client, "Теперь ты администратор.", null);
        }
    }
}
