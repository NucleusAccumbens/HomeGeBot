namespace Bot.Common.Interfaces;

public interface IUserStatusChecker
{
    Task<bool> IsKickedAsync(long? chatId, CancellationToken cancellationToken = default);
}
