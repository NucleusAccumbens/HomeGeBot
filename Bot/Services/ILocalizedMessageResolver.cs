namespace Bot.Services;

public interface ILocalizedMessageResolver
{
    Task<string> ResolveAsync(long chatId, string messageName, string fallback, CancellationToken cancellationToken = default);
}
