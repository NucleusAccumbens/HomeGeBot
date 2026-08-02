namespace Bot.Services;

public class BotI18n : IBotI18n
{
    private static readonly Dictionary<string, Dictionary<string, string>> _translations = new()
    {
        ["ru"] = new()
        {
            ["btn.openApplication"] = "Открыть заявку",
            ["btn.openApp"] = "Открыть приложение",
            ["btn.cancelApplication"] = "✖️ Отменить заявку",
            ["btn.write"] = "Написать",
            ["btn.messageManager"] = "Написать менеджеру",
        },
        ["en"] = new()
        {
            ["btn.openApplication"] = "Open application",
            ["btn.openApp"] = "Open app",
            ["btn.cancelApplication"] = "✖️ Cancel application",
            ["btn.write"] = "Write",
            ["btn.messageManager"] = "Message manager",
        },
        ["ka"] = new()
        {
            ["btn.openApplication"] = "განაცხადის გახსნა",
            ["btn.openApp"] = "აპის გახსნა",
            ["btn.cancelApplication"] = "✖️ განაცხადის გაუქმება",
            ["btn.write"] = "დაწერა",
            ["btn.messageManager"] = "მენეჯერთან მიწერა",
        }
    };

    public string T(string key, string language = "ru")
    {
        if (_translations.TryGetValue(language, out var lang) && lang.TryGetValue(key, out var value))
            return value;
        if (_translations["ru"].TryGetValue(key, out var fallback))
            return fallback;
        return key;
    }
}
