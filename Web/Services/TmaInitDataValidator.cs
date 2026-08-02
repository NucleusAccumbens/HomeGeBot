using System.Security.Cryptography;
using System.Text;
using Bot.Configuration;
using Microsoft.Extensions.Options;

namespace Web.Services;

public class TmaInitDataValidator : ITmaInitDataValidator
{
    private const string TelegramMiniAppDataKey = "WebAppData";

    private readonly TelegramBotConfiguration _botConfig;

    public TmaInitDataValidator(IOptions<TelegramBotConfiguration> botConfig)
    {
        _botConfig = botConfig.Value;
    }

    public bool Validate(IReadOnlyDictionary<string, string> data)
    {
        if (data.Count == 0 || !data.TryGetValue("hash", out var hash))
            return false;

        var dataCheckString = string.Join("\n", data
            .Where(x => x.Key != "hash")
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Value}"));

        using var hmacSecret = new HMACSHA256(Encoding.UTF8.GetBytes(TelegramMiniAppDataKey));
        var secretKey = hmacSecret.ComputeHash(Encoding.UTF8.GetBytes(_botConfig.Token));

        using var hmacData = new HMACSHA256(secretKey);
        var checkHash = BitConverter.ToString(hmacData.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString)))
            .Replace("-", string.Empty)
            .ToLower();

        return checkHash == hash;
    }
}
