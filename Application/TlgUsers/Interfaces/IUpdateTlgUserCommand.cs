namespace Application.TlgUsers.Interfaces;

public interface IUpdateTlgUserCommand
{
    Task UpdateTlgUserIsAdminAsync(long chatId, bool isAdmin);

    Task UpdateUsernameAsync(long chatId, string username);
}
