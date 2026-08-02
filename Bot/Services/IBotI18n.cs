namespace Bot.Services;

public interface IBotI18n
{
    string T(string key, string language = "ru");
}
