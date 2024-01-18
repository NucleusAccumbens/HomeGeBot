using Telegram.Bot.Types;

namespace Application.TlgUsers.Interfaces;

public interface ICreateTlgUserCommand
{
    Task CreateTlgUserAsync(Chat chat);
}
