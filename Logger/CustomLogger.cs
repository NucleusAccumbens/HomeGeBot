using Logger.Interfaces;
using NLog;
using System.Text.RegularExpressions;

namespace Logger;

public class CustomLogger : ICustomLogger
{
    private readonly ILogger _nLogLogger;

    public CustomLogger()
    {
        var logPath = Path.Combine(Directory.GetCurrentDirectory(),
            "logs", DateTime.Now.ToString("dd.MM.yyyy"));
        EnsureDirectoryExists(logPath);

        new LoggerConfigurator()
            .SetFileName(Path.Combine(logPath, "log_file.log"))
            .SetLayout("${longdate} ${level} ${message} ${exception:format=ToString}")
            .AddConsoleTarget("${longdate} ${level} ${message} ${exception:format=ToString}")
            .BuildConfiguration();

        _nLogLogger = LogManager.GetCurrentClassLogger();
    }

    public void LogAction(string action)
    {
        string logMessage = action;
        _nLogLogger.Info(logMessage);
    }

    public void LogError(Exception ex)
    {
        string logMessage = $"Ошибка: {ex.Message}";
        _nLogLogger.Error(logMessage);
    }

    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании папки {directoryPath}: {ex.Message}");
            }
        }
    }
}
