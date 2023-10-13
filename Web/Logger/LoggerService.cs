using NLog;
using NLog.Config;
using NLog.Targets;
using System.Text.RegularExpressions;

namespace Web.Logger;

public static class LoggerService
{
    private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

    public static void ConfigureLogger()
    {
        var config = new LoggingConfiguration();

        var target = new FileTarget("fileTarget")
        {
            FileName = @"..\..\..\log_file.log", // Укажите путь к файлу логов
            Layout = "${longdate} ${level} ${message} ${exception:format=ToString}", // Формат записи лога
            ArchiveEvery = FileArchivePeriod.Month, // Архивировать логи каждый месяц
            ArchiveNumbering = ArchiveNumberingMode.Rolling, // Режим архивирования (перезаписывать старые логи)
            MaxArchiveFiles = 7 // Максимальное количество архивных файлов 
        };

        config.AddTarget(target);

        var rule = new LoggingRule("*", NLog.LogLevel.Debug, target);
        config.LoggingRules.Add(rule);

        LogManager.Configuration = config;
    }

    public static void LogAction(string userName, string action)
    {
        string logMessage = $"[{userName}] {action}";
        Logger.Info(logMessage);    }

    public static void LogError(Exception ex)
    {
        string logMessage = $"Ошибка: {ex.Message}";
        Logger.Error(logMessage); 
    }
}
