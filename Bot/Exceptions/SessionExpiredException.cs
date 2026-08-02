namespace Bot.Exceptions;

public class SessionExpiredException : Exception
{
    public static readonly string MessageText =
        "Прошло слишком много времени с предыдущего сеанса, " +
        "данные не сохранились.\n\n" +
        "Чтобы начать сначала, нажмите /start";

    public SessionExpiredException() : base() { }
}
