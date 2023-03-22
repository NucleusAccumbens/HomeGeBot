using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Parser.Services;

namespace Parser.Parsers;

public class HomeGeParser
{
    private readonly string _baseUrl;

    private readonly HtmlLoadService _htmlLoader;

    public HomeGeParser(string baseUrl)
    {
        _baseUrl = baseUrl;
        _htmlLoader = new(_baseUrl);
    }

    public async Task<string?> GetLatestItemId(string postfix)
    {
        try
        {
            var document = await GetHtmlDocumentByPostfix(postfix);

            var priceItem = document.QuerySelectorAll("div")
                .Where(item => item.ClassName != null && item.ClassName.Contains("statement-card"))
                .FirstOrDefault();

            return priceItem?.GetAttribute("data-product-id");
        }
        catch (Exception ex) 
        { 
            Console.WriteLine(ex.Message); 
            return null;
        }
    }

    public async Task<string?> GetItemUrl(string postfix, string id)
    {
        var document = await GetHtmlDocumentByPostfix(postfix);

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

    public async Task<string?> GetOwnerNumber(string url)
    {
        var document = await GetHtmlDocument(url);

        var links = document.QuerySelectorAll("a");

        foreach (var link in links)
        {
            if (link.Attributes["href"] != null && link.Attributes["href"].Value.Contains("tel"))
            {
                string? res = link?.Attributes["href"]?.Value;

                if (res != null && res.Length <= 16)
                {
                    return res[7..];
                }

                else if (res != null && res.Contains('+')) return res[8..];
            }
        }

        return null;
    }

    private async Task<IHtmlDocument> GetHtmlDocumentByPostfix(string postfix)
    {
        string? sourse = await _htmlLoader.GetSourceByPostfixUrl(postfix);

        var parser = new HtmlParser();

        return parser.ParseDocument(sourse);
    }

    private async Task<IHtmlDocument> GetHtmlDocument(string url)
    {
        string? sourse = await _htmlLoader.GetSourceByUrl(url);

        var parser = new HtmlParser();

        return parser.ParseDocument(sourse);
    }
}
