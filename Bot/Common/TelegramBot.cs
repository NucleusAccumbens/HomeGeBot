using Bot.Configuration;
using Bot.Exceptions;
using Bot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bot.Common;

public interface IWebhookSetupService
{
    Task SetupWebhookAsync(ITelegramBotClient client);
}

public class WebhookSetupService : IWebhookSetupService
{
    private readonly WebhookConfiguration _webhookConfig;
    private readonly ILogger<WebhookSetupService> _logger;

    public WebhookSetupService(IOptions<WebhookConfiguration> webhookConfig, ILogger<WebhookSetupService> logger)
    {
        _webhookConfig = webhookConfig.Value;
        _logger = logger;
    }

    public async Task SetupWebhookAsync(ITelegramBotClient client)
    {
        var baseUrl = _webhookConfig.BaseUrl;

        if (string.IsNullOrEmpty(baseUrl))
            throw new UrlException("Webhook BaseUrl отсутствует в файле конфигурации");

        await client.SetWebhookAsync($"{baseUrl}api/message/update");

        var me = await client.GetMeAsync();
        _logger.LogInformation("Начал принимать обновления из чатов с ботом @{Username}", me.Username);
    }
}

public class TelegramBot : ITelegramBotClientProvider
{
    private readonly TelegramBotConfiguration _botConfig;
    private readonly IWebhookSetupService _webhookSetupService;
    private TelegramBotClient? _client;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public TelegramBot(IOptions<TelegramBotConfiguration> botConfig, IWebhookSetupService webhookSetupService)
    {
        _botConfig = botConfig.Value;
        _webhookSetupService = webhookSetupService;
    }

    public async Task<TelegramBotClient> GetBot()
    {
        if (_client != null)
            return _client;

        await _initLock.WaitAsync();
        try
        {
            if (_client != null)
                return _client;

            var token = _botConfig.Token;

            if (string.IsNullOrEmpty(token))
                throw new TokenException("Токен отсутствует в файле конфигурации");

            var client = new TelegramBotClient(token);

            await _webhookSetupService.SetupWebhookAsync(client);

            _client = client;
            return _client;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
