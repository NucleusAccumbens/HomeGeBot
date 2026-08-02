using Bot.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Web.Services;

public class BotInitializationService : IHostedService
{
    private readonly TelegramBot _bot;
    private readonly ILogger<BotInitializationService> _logger;

    public BotInitializationService(TelegramBot bot, ILogger<BotInitializationService> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _bot.GetBot();
        }
        catch (Exception ex)
        {
            // Bot initialization failed (e.g. Telegram API unreachable).
            // The web app should still start so the UI is accessible.
            _logger.LogError(ex, "Bot initialization failed; web app will start without bot connectivity");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
