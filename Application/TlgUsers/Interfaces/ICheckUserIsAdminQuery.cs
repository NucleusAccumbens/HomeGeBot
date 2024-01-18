namespace Application.TlgUsers.Interfaces;

public interface ICheckUserIsAdminQuery
{
    Task<bool?> CheckUserIsAdminAsync(long chatId);
}
