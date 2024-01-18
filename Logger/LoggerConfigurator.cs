using Logger.Interfaces;
using NLog.Config;
using NLog.Targets;
using NLog;

namespace Logger;

public class LoggerConfigurator : ILoggerConfigurator
{
    private readonly LoggingConfiguration _config;
    private readonly FileTarget _target;
    private NLog.LogLevel _logLevel = NLog.LogLevel.Debug;

    public LoggerConfigurator()
    {
        _config = new LoggingConfiguration();
        _target = new FileTarget("fileTarget");
        _config.AddTarget(_target);
    }

    public ILoggerConfigurator AddConsoleTarget(string layout)
    {
        var consoleTarget = new ColoredConsoleTarget("consoleTarget")
        {
            Layout = layout
        };
        _config.AddTarget(consoleTarget);

        var consoleRule = new LoggingRule("*", _logLevel, consoleTarget);
        _config.LoggingRules.Add(consoleRule);

        return this;
    }

    public ILoggerConfigurator SetFileName(string fileName)
    {
        _target.FileName = fileName;
        return this;
    }

    public ILoggerConfigurator SetLayout(string layout)
    {
        _target.Layout = layout;
        return this;
    }

    public ILoggerConfigurator SetArchiveEvery(FileArchivePeriod archivePeriod)
    {
        _target.ArchiveEvery = archivePeriod;
        return this;
    }

    public ILoggerConfigurator SetArchiveNumbering(ArchiveNumberingMode numberingMode)
    {
        _target.ArchiveNumbering = numberingMode;
        return this;
    }

    public ILoggerConfigurator SetMaxArchiveFiles(int maxArchiveFiles)
    {
        _target.MaxArchiveFiles = maxArchiveFiles;
        return this;
    }

    public ILoggerConfigurator SetLogLevel(NLog.LogLevel logLevel)
    {
        _logLevel = logLevel;
        return this;
    }

    public void BuildConfiguration()
    {
        var rule = new LoggingRule("*", _logLevel, _target);
        _config.LoggingRules.Add(rule);
        LogManager.Configuration = _config;
    }
}
