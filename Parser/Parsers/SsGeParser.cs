using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Parser.Services;

namespace Parser.Parsers;

public class SsGeParser
{
    private readonly string _baseUrl;

    private readonly HtmlLoadService _htmlLoader;

    public SsGeParser(string baseUrl)
    {
        _baseUrl = baseUrl;
        _htmlLoader = new(_baseUrl);
    }

    public async Task<string?> GetLatestItemId(string postfix)
    {
        var document = await GetHtmlDocument(postfix);

        var priceItem = document.QuerySelectorAll("div")
            .Where(item => item.ClassName != null && item.ClassName.Contains("latest_article_each "))
            .FirstOrDefault();

        if (priceItem == null) { Console.WriteLine("\n\nGetLastestItemId SSGeParser вернул null\n\n"); }

        return priceItem?.GetAttribute("data-id");
    }

    public async Task<string?> GetItemUrlPostfix(string postfix, string id)
    {
        var document = await GetHtmlDocument(postfix);

        var links = document.QuerySelectorAll("a");

        foreach (var link in links)
        {
            if (link.Attributes["href"] != null && link.Attributes["href"].Value.Contains(id))
            {
                return link.Attributes["href"].Value;
            }
        }

        return null;
    }

    public async Task<string?> GetOwnerNumber(string postfix)
    {
        var document = await GetHtmlDocument(postfix);

        var priceItem = document.QuerySelectorAll("span")
            .Where(item => item.ClassName != null && item.ClassName.Contains("EAchPHonenumber"))
            .FirstOrDefault();

        return priceItem?.TextContent;
    }

    private async Task<IHtmlDocument> GetHtmlDocument(string postfix)
    {
        string? sourse = await _htmlLoader.GetSourceByPostfixUrl(postfix);

        var parser = new HtmlParser();

        return parser.ParseDocument(sourse);
    }
}
