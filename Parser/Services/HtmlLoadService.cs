using System.Net;
using Parser.Settings;

namespace Parser.Services;

internal class HtmlLoadService
{
    private readonly HttpClient _client;

    private readonly ParserSettings _settings;

    public HtmlLoadService(string baseUrl)
    {
        _client = new HttpClient() { Timeout = TimeSpan.FromMinutes(3) };
        _settings = new(baseUrl);
    }

    public async Task<string?> GetSourceByPostfixUrl(string postfix)
    {
        var response = await _client.GetAsync(_settings.GetFullUrl(postfix));

        if (response != null && response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return null;
    }

    public async Task<string?> GetSourceByUrl(string url)
    {      
        var response = await _client.GetAsync(url);

        if (response != null && response.StatusCode == HttpStatusCode.OK)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return null;
    }
}
