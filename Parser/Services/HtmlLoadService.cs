using System.Net;
using Parser.Exceptions;
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

    public async Task<string> GetSourceByPostfixUrl(string postfix)
    {
        string url = _settings.GetFullUrl(postfix);

        var response = await _client.GetAsync(url);

        if (response == null) 
        {
            throw new NoResponseException();
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new BadResponseException(response);
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetSourceByUrl(string url)
    {
        var response = await _client.GetAsync(url);

        if (response == null)
        {
            throw new NoResponseException();
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new BadResponseException(response);
        }

        return await response.Content.ReadAsStringAsync();
    }
}
