namespace EasyBackend.Utils;

public enum LogLevel
{
    Debug,
    Info,
    Warn,
    Error,
    Fatal
}

public class Logger
{
    private Dictionary<string, ILogHandler> _logHandlers = new();

    internal void Init(AppConfig config)
    {
    }

    public void Debug(string message) => LogImpl(LogLevel.Debug, message);
    public void Info(string message) => LogImpl(LogLevel.Info, message);
    public void Warn(string message) => LogImpl(LogLevel.Warn, message);
    public void Error(string message) => LogImpl(LogLevel.Error, message);
    public void Fatal(string message) => LogImpl(LogLevel.Fatal, message);

    private void LogImpl(LogLevel level, string message)
    {
        foreach (var handler in _logHandlers.Values)
        {
            handler?.DoLog(level, message);
        }
    }
}

interface ILogHandler
{
    void DoLog(LogLevel level, string message);
}
