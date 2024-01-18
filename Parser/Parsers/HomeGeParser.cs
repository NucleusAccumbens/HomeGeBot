using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Parser.Services;
using System.Text.RegularExpressions;

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

            // Изменение селектора для поиска элементов div с атрибутом data-product-id
            var cardItem = document.QuerySelectorAll("div")
                    .FirstOrDefault();

            return cardItem?.GetAttribute("data-product-id");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string?> GetItemUrl(string postfix, string id)
    {
        try
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
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string?> GetOwnerNumber(string url)
    {
        try
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
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<IHtmlDocument> GetHtmlDocumentByPostfix(string postfix)
    {
        try
        {
            string sourse = await _htmlLoader.GetSourceByPostfixUrl(postfix);

            var parser = new HtmlParser();

            return parser.ParseDocument(sourse);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<IHtmlDocument> GetHtmlDocument(string url)
    {
        try
        {
            string sourse = await _htmlLoader.GetSourceByUrl(url);

            var parser = new HtmlParser();

            return parser.ParseDocument(sourse);
        }
        catch (Exception) 
        {
            throw;
        }
    }
}
