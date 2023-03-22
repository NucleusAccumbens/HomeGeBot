using Application.Admins.Interfaces;
using Application.Flats.Interfaces;
using Domain.Entities;
using Parser.Parsers;
using Telegram.Bot;

namespace Web.BackgroundTasks;

internal interface IScopedProcessingService
{
    Task DoWork(CancellationToken stoppingToken);
}

internal class ScopedProcessingService : IScopedProcessingService
{
    private int executionCount = 0;

    private readonly ILogger _logger;

    private readonly ICheckFlatIsInBdQuery _checkFlatIsInBdQuery;

    private readonly ICreateFlatCommand _createFlatCommand;

    private readonly IGetAdminsQuery _getAdminsQuery;

    private readonly ITelegramBotClient _client = new TelegramBotClient("6123649331:AAH5GpUd2w5KarrU-LZanqMTfq4IDI6viWE");

    private readonly string _ssPostfix 
        = "/en/real-estate/l/Flat/For-Rent?MunicipalityId=95&CityIdList=95" +
        "&CommercialRealEstateType=&PriceType=false&CurrencyId=1" +
        "&Context.Request.Query%5BQuery%5D=&IndividualEntityOnly=true&";

    private readonly string _homeGePostfix =
        "/en/s/Apartment-for-rent-House-for-rent-Tbilisi?Keyword=Tbilisi&AdTypeID=3&PrTypeID=1.2&mapC=41.73188365%2C44.8368762993663&cities=1996871&GID=1996871&OwnerTypeID=1";

    private readonly SsGeParser _ssParser = new("https://ss.ge");

    private readonly HomeGeParser _homeGeParser = new("https://www.myhome.ge");

    public ScopedProcessingService(ILogger<ScopedProcessingService> logger,
        ICheckFlatIsInBdQuery checkFlatIsInBdQuery, ICreateFlatCommand createFlatCommand,
        IGetAdminsQuery getAdminsQuery)
    {
        _logger = logger;
        _checkFlatIsInBdQuery = checkFlatIsInBdQuery;
        _createFlatCommand = createFlatCommand;
        _getAdminsQuery = getAdminsQuery;
    }

    public async Task DoWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            executionCount++;

            _logger.LogInformation(
                "Scoped Processing Service is working. Count: {Count}", executionCount);

            await AddNewSsGeFlatInDb();

            await AddNewHomeGeFlatInDb();

            PingSite();

            await Task.Delay(new TimeSpan(0, 0, 10), stoppingToken);
        }
    }

    private async Task AddNewSsGeFlatInDb()
    {
        string? newFlatId = await GetLastestFlatIdFromSsGe();

        if (newFlatId != null) 
        {
            string flatUrlPostfix = await _ssParser.GetItemUrlPostfix(_ssPostfix, newFlatId);

            string ownerNumber = await _ssParser.GetOwnerNumber(flatUrlPostfix);
            
            var newFlat = new Flat()
            {
                ItemId = newFlatId,
                OwnerNumber = ownerNumber,
                Link = $"https://ss.ge{flatUrlPostfix}"
            };

            await _createFlatCommand.CreateFlatAsync(newFlat);

            await SendNotifyToAdmins($"https://ss.ge{flatUrlPostfix}", "ss.ge");
        }
    }

    private async Task AddNewHomeGeFlatInDb()
    {
        string? newFlatId = await GetLastestFlatIdFromHomeGe();

        if (newFlatId != null)
        {
            string flatUrl = await _homeGeParser.GetItemUrl(_homeGePostfix, newFlatId);

            string ownerNumber = await _homeGeParser.GetOwnerNumber(flatUrl);

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

    private async Task<string?> GetLastestFlatIdFromSsGe()
    {
        string? lastestItemId = await _ssParser.GetLatestItemId(_ssPostfix);

        if (lastestItemId != null)
        {
            bool isInDb = await _checkFlatIsInBdQuery.CheckFlatIsInBdAsync(lastestItemId);

            if (isInDb) return null;

            else return lastestItemId;
        }

        else throw new NullReferenceException();
    }

    private async Task<string?> GetLastestFlatIdFromHomeGe()
    {
        string? lastestItemId = await _homeGeParser.GetLatestItemId(_homeGePostfix);

        if (lastestItemId != null)
        {
            bool isInDb = await _checkFlatIsInBdQuery.CheckFlatIsInBdAsync(lastestItemId);

            if (isInDb) return null;

            else return lastestItemId;
        }

        else throw new NullReferenceException();
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

    private static void PingSite()
    {
        try
        {
            var client = new HttpClient();

            var res = client.GetAsync("https://noncredist.bsite.net/").Result;

            Console.WriteLine($"{res.Content.Headers}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}
