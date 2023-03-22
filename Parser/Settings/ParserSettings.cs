namespace Parser.Settings;

public class ParserSettings
{
    private readonly string _baseUrl;

    public ParserSettings(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public string GetFullUrl(string postfix)
    {
        return $"{_baseUrl}{postfix}";
    }
}
