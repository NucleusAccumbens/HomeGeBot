using Bot.Common.Interfaces;
using Logger.Interfaces;
using Telegram.Bot;

namespace Web.BackgroundTasks;

public class ConsumeScopedHostedService : BackgroundService
{
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ICustomLogger _logger;
    private readonly ITelegramBotClient _client;
    
    public ConsumeScopedHostedService(IServiceProvider services, ICustomLogger logger,
        IConfiguration configuration, IExceptionNotification exceptionNotification)
    {
        Services = services;
        _logger = logger;
        _exceptionNotification = exceptionNotification;
        _client = new TelegramBotClient(configuration["Token"]);
    }

    public IServiceProvider Services { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogAction("Consume Scoped Service Hosted Service running.");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogAction("Consume Scoped Service Hosted Service is working.");

            using var scope = Services.CreateScope();
            var scopedProcessingService =
                scope.ServiceProvider
                    .GetRequiredService<IScopedProcessingService>();

            await scopedProcessingService.DoWork(stoppingToken);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex);
            await _exceptionNotification.SendExceptionNotification(_client, ex.Message,
                444343256, 2030541425);

            await StopAsync(stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogAction("Consume Scoped Service Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
