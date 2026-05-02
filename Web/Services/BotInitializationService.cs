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
        await _bot.GetBot();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
