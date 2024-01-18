namespace Application.TlgUsers.Interfaces;

public interface ICheckUserIsInDbQuery
{
    Task<bool> CheckUserIsInDbAsync(long chatId);
}

