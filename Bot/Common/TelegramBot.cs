using Bot.Configuration;
using Bot.Exceptions;
using Microsoft.Extensions.Options;

namespace Bot.Common;

public class TelegramBot
{
    private readonly TelegramBotConfiguration _botConfig;
    private readonly WebhookConfiguration _webhookConfig;
    private TelegramBotClient? _client;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public TelegramBot(IOptions<TelegramBotConfiguration> botConfig, IOptions<WebhookConfiguration> webhookConfig)
    {
        _botConfig = botConfig.Value;
        _webhookConfig = webhookConfig.Value;
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

            await SetWebhookAsync(client);
            await NotifyAboutAcceptingUpdates(client);

            _client = client;
            return _client;
        }
        catch (UrlException)
        {
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task SetWebhookAsync(TelegramBotClient client)
    {
        var baseUrl = _webhookConfig.BaseUrl;

        if (!string.IsNullOrEmpty(baseUrl))
        {
            await client.SetWebhookAsync($"{baseUrl}api/message/update");
        }
        else throw new UrlException("Webhook BaseUrl отсутствует в файле конфигурации");
    }

    private static async Task NotifyAboutAcceptingUpdates(TelegramBotClient client)
    {
        var me = await client.GetMeAsync();

        Console.WriteLine($"Начал принимать обновления из чатов с ботом @{me.Username}");
    }
}
