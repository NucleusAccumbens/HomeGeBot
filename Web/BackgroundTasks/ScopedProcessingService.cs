using AngleSharp.Dom;
using Application.Admins.Interfaces;
using Application.Flats.Interfaces;
using Bot.Common.Interfaces;
using Domain.Entities;
using Logger.Interfaces;
using Parser.Parsers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace Web.BackgroundTasks;

internal interface IScopedProcessingService
{
    Task DoWork(CancellationToken stoppingToken);
}

internal class ScopedProcessingService : IScopedProcessingService
{
    private readonly IExceptionNotification _exceptionNotification;
    private readonly ICustomLogger _logger;
    private readonly ICheckFlatIsInBdQuery _checkFlatIsInBdQuery;
    private readonly ICreateFlatCommand _createFlatCommand;
    private readonly IGetAdminsQuery _getAdminsQuery;
    private readonly ITelegramBotClient _client;
    private readonly string _ssPostfix;
    private readonly string _homeGePostfix;
    private readonly SsGeParser _ssParser;
    private readonly HomeGeParser _homeGeParser;
    private int _executionCount = 0;

    public ScopedProcessingService(ICustomLogger logger, ICheckFlatIsInBdQuery checkFlatIsInBdQuery,
    ICreateFlatCommand createFlatCommand, IGetAdminsQuery getAdminsQuery, 
    IConfiguration configuration, IExceptionNotification exceptionNotification)
    {
        _logger = logger;
        _checkFlatIsInBdQuery = checkFlatIsInBdQuery;
        _createFlatCommand = createFlatCommand;
        _getAdminsQuery = getAdminsQuery;
        _exceptionNotification = exceptionNotification;
        _client = new TelegramBotClient(configuration["Token"]);
        _ssPostfix = configuration["Postfixes:ssPostfix"];
        _homeGePostfix = configuration["Postfixes:homeGePostfix"];
        _ssParser = new SsGeParser(configuration["BaseUrls:ss"]);
        _homeGeParser = new HomeGeParser(configuration["BaseUrls:myhome"]);
    }

    public async Task DoWork(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await AddNewHomeGeFlatInDb();

                _executionCount++;
                _logger.LogAction($"Scoped Processing Service is working. Count: {_executionCount}");

                await Task.Delay(new TimeSpan(0, 0, 40), stoppingToken);
            }
        }
        catch(Exception) 
        {
            throw;
        }
    }

    private async Task AddNewHomeGeFlatInDb()
    {
        try
        {
            string? newFlatId = await GetLastestFlatIdFromHomeGe();

            if (newFlatId != null)
            {
                string? flatUrl = await _homeGeParser.GetItemUrl(_homeGePostfix, newFlatId);

                if (flatUrl == null) throw new NullReferenceException(
                    "Не удалось получить ссылку на объявление.");

                string? ownerNumber = await _homeGeParser.GetOwnerNumber(flatUrl);

                if (flatUrl == null) throw new NullReferenceException(
                    "Не удалось получить номер владельца.");

                var newFlat = new Flat()
                {
                    ItemId = newFlatId,
                    OwnerNumber = ownerNumber,
                    Link = $"{flatUrl}"
                };

                await _createFlatCommand.CreateFlatAsync(newFlat);

                await SendNotifyToAdmins(flatUrl, "myhome.ge");
            }
        }
        catch(Exception)
        {
            throw;
        }
    }

    private async Task<string?> GetLastestFlatIdFromHomeGe()
    {
        try
        {
            string? lastestItemId = await _homeGeParser.GetLatestItemId(_homeGePostfix);

            if (lastestItemId == null) throw new NullReferenceException(
                "Не удалось получить ID объявления.");

            if (lastestItemId != null)
            {
                bool isInDb = await _checkFlatIsInBdQuery.CheckFlatIsInBdAsync(lastestItemId);

                if (isInDb) return null;

                else return lastestItemId;
            }

            else return null;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task SendNotifyToAdmins(string url, string site)
    {
        var adminsChatIds = await _getAdminsQuery.GetAdminsChatIdsAsync();

        foreach (var adminChatId in adminsChatIds) 
        {
            await _client.SendTextMessageAsync(
                chatId: adminChatId,
                text: $"<a href=\"{url}\">Новое объявление</a> на сайте {site}.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                disableWebPagePreview: true);
        }
    }
}
