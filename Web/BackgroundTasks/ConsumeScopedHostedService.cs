namespace Web.BackgroundTasks;

public class ConsumeScopedHostedService : BackgroundService
{
    private readonly ILogger<ConsumeScopedHostedService> _logger;

    public ConsumeScopedHostedService(IServiceProvider services,
        ILogger<ConsumeScopedHostedService> logger)
    {
        Services = services;
        _logger = logger;
    }

    public IServiceProvider Services { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service running.");

        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service is working.");

        using var scope = Services.CreateScope();
        var scopedProcessingService =
            scope.ServiceProvider
                .GetRequiredService<IScopedProcessingService>();

        await scopedProcessingService.DoWork(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Consume Scoped Service Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}
