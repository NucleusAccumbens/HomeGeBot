namespace Application.TlgUsers.Interfaces;

public interface IKickTlgUserCommand
{
    Task ManageTlgUserKickingAsync(long chatId);

    Task<bool> CheckTlgUserIsKicked(long? chatId);
}
