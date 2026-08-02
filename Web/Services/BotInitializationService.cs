using Bot.Common;
using Microsoft.Extensions.Hosting;

namespace Web.Services;

public class BotInitializationService : IHostedService
{
    private readonly TelegramBot _bot;

    public BotInitializationService(TelegramBot bot)
    {
        _bot = bot;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _bot.GetBot();
        }
        catch (Exception)
        {
            // Bot initialization failed (e.g. Telegram API unreachable).
            // The web app should still start so the UI is accessible.
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
