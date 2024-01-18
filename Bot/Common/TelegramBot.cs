using Bot.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Bot.Common;

public class TelegramBot
{
    private readonly IConfiguration _configuration;
    private TelegramBotClient? _client;

    public TelegramBot(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<TelegramBotClient> GetBot()
    {
        if (_client != null)
        {
            return _client;
        }

        string? token = _configuration.GetValue<string>("Token");

        try
        {
            if (token != null)
            {
                _client = new TelegramBotClient(token);

                await SetWebhookAsync(_client);

                NotifyAboutAcceptingUpdates(_client);

                return _client;
            }
            else throw new TokenException("Токен отсутствует в файле конфигурации");
        }  
        catch (UrlException)
        {
            throw;
        }
    }

    private async Task SetWebhookAsync(TelegramBotClient client)
    {
        string? baseUrl = _configuration.GetValue<string>("Url");

        if (baseUrl != null) 
        {
            await client.SetWebhookAsync($"{baseUrl}api/message/update");
        }

        else throw new UrlException("Url отсутствует в файле конфигурации");
    }

    private static void NotifyAboutAcceptingUpdates(TelegramBotClient client)
    {
        var me = client.GetMeAsync().Result;

        Console.WriteLine($"Начал принимать обновления из чатов с ботом @{me.Username}");
    }
}
