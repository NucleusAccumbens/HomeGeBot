using NLog.Targets;

namespace Logger.Interfaces;

public interface ILoggerConfigurator
{
    ILoggerConfigurator AddConsoleTarget(string layout);
    ILoggerConfigurator SetFileName(string fileName);
    ILoggerConfigurator SetLayout(string layout);
    ILoggerConfigurator SetArchiveEvery(FileArchivePeriod archivePeriod);
    ILoggerConfigurator SetArchiveNumbering(ArchiveNumberingMode numberingMode);
    ILoggerConfigurator SetMaxArchiveFiles(int maxArchiveFiles);
    ILoggerConfigurator SetLogLevel(NLog.LogLevel logLevel);
    void BuildConfiguration();
}
