namespace Application.Common.Localization;

public static class ApplicationMessages
{
    public static string ApplicationLimitExceeded(string language) => language switch
    {
        "en" => "You have already submitted 5 applications — this is the maximum.",
        "ka" => "თქვენ უკვე გაგზავნეთ 5 განაცხადი — ეს მაქსიმალური რაოდენობაა.",
        _ => "Вы уже отправили 5 заявок — это максимальное количество."
    };

    public static string NoActiveManagers(string language) => language switch
    {
        "en" => "No active managers available.",
        "ka" => "აქტიური მენეჯერები არ არის.",
        _ => "Нет активных менеджеров"
    };
}
