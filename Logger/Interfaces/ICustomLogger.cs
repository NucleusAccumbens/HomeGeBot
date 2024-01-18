namespace Logger.Interfaces;

public interface ICustomLogger
{
    void LogAction(string action);
    void LogError(Exception ex);
}
