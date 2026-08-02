using System.Security.Cryptography;
using System.Text;
using Bot.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Web.Services;

public class TmaValidationService : ITmaValidationService
{
    private readonly TelegramBotConfiguration _botConfig;

    public TmaValidationService(IOptions<TelegramBotConfiguration> botConfig)
    {
        _botConfig = botConfig.Value;
    }

    public bool ValidateInitData(string initData)
    {
        if (string.IsNullOrEmpty(initData)) return false;

        var data = ParseInitData(initData);
        if (!data.ContainsKey("hash")) return false;

        var hash = data["hash"];
        data.Remove("hash");

        var dataCheckString = string.Join("\n", data.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));

        using var hmacSecret = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData"));
        var secretKey = hmacSecret.ComputeHash(Encoding.UTF8.GetBytes(_botConfig.Token));

        using var hmacData = new HMACSHA256(secretKey);
        var checkHash = BitConverter.ToString(hmacData.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString))).Replace("-", "").ToLower();

        return checkHash == hash;
    }

    public long? GetUserId(string initData)
    {
        var data = ParseInitData(initData);
        if (data.TryGetValue("user", out var userJson))
        {
            var user = JObject.Parse(userJson);
            return user["id"]?.Value<long>();
        }
        return null;
    }

    public string? GetUsername(string initData)
    {
        var data = ParseInitData(initData);
        if (data.TryGetValue("user", out var userJson))
        {
            var user = JObject.Parse(userJson);
            return user["username"]?.Value<string>() ?? user["first_name"]?.Value<string>();
        }
        return null;
    }

    public TmaUserData? GetUserData(string initData)
    {
        var data = ParseInitData(initData);
        if (!data.TryGetValue("user", out var userJson)) return null;
        var user = JObject.Parse(userJson);
        return new TmaUserData(
            Id: user["id"]?.Value<long>() ?? 0,
            Username: user["username"]?.Value<string>(),
            FirstName: user["first_name"]?.Value<string>(),
            LastName: user["last_name"]?.Value<string>(),
            PhotoUrl: user["photo_url"]?.Value<string>()
        );
    }

    private Dictionary<string, string> ParseInitData(string initData)
    {
        return initData.Split('&')
            .Select(x => x.Split('='))
            .ToDictionary(x => x[0], x => Uri.UnescapeDataString(x[1]));
    }
}
