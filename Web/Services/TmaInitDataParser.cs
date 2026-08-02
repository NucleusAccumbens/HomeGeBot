using Newtonsoft.Json.Linq;

namespace Web.Services;

public class TmaInitDataParser : ITmaInitDataParser
{
    public IReadOnlyDictionary<string, string> Parse(string initData)
    {
        if (string.IsNullOrEmpty(initData))
            return new Dictionary<string, string>();

        return initData.Split('&')
            .Select(x => x.Split('='))
            .ToDictionary(x => x[0], x => Uri.UnescapeDataString(x[1]));
    }

    public TmaUserData? GetUserData(IReadOnlyDictionary<string, string> data)
    {
        if (!data.TryGetValue("user", out var userJson))
            return null;

        var user = JObject.Parse(userJson);
        var id = user["id"]?.Value<long>();

        if (id is null or 0)
            return null;

        return new TmaUserData(
            Id: id.Value,
            Username: user["username"]?.Value<string>(),
            FirstName: user["first_name"]?.Value<string>(),
            LastName: user["last_name"]?.Value<string>(),
            PhotoUrl: user["photo_url"]?.Value<string>()
        );
    }
}
