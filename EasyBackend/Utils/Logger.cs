using System.Text;

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
    private readonly Dictionary<string, ILogHandler> _logHandlers = new();

    internal void Init(AppConfigLogging config)
    {
        if (config.ConsoleEnabled)
        {
            var consoleHandler = new ConsoleLogHandler
            {
                EnableColor = config.ConsoleColor
            };
            AppendHandler("console", consoleHandler);
        }

        if (!string.IsNullOrEmpty(config.LogFileFolder) &&
            Directory.Exists(config.LogFileFolder))
        {
            var fileHandler = new FileLogHandler(config.LogFileFolder);
            AppendHandler("file", fileHandler);
        }

        LogImpl(LogLevel.Info, "===== Logger initialized =====", "Logger");
    }

    public void Cleanup()
    {
        lock (_logHandlers)
        {
            foreach (var handler in _logHandlers.Values)
            {
                try
                {
                    handler?.Teardown();
                }
                catch (Exception)
                {
                    // Ignore any exceptions during teardown
                }
            }
            _logHandlers.Clear();
        }
    }

    public void AppendHandler(string name, ILogHandler handler)
    {
        try
        {
            handler.Setup();
            lock (_logHandlers)
            {
                _logHandlers.Add(name, handler);
            }
        }
        catch (Exception)
        {
            // Setup失败，忽略该handler
        }
    }

    public void Debug(string message, string module = null) => LogImpl(LogLevel.Debug, message, module);
    public void Info(string message, string module = null) => LogImpl(LogLevel.Info, message, module);
    public void Warn(string message, string module = null) => LogImpl(LogLevel.Warn, message, module);
    public void Error(string message, string module = null) => LogImpl(LogLevel.Error, message, module);
    public void Fatal(string message, string module = null) => LogImpl(LogLevel.Fatal, message, module);

    private void LogImpl(LogLevel level, string message, string module)
    {
        if (!string.IsNullOrEmpty(module))
        {
            message = $"[{module}]" + message;
        }

        lock (_logHandlers)
        {
            foreach (var handler in _logHandlers.Values)
            {
                handler?.DoLog(level, message);
            }
        }
    }
}

public interface ILogHandler
{
    void Setup();
    void Teardown();
    void DoLog(LogLevel level, string message);
}

internal class ConsoleLogHandler : ILogHandler
{
    public bool EnableColor { get; set; }
    private const string Format = "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}";

    public void Setup()
    {
    }

    public void Teardown()
    {
    }

    public void DoLog(LogLevel level, string message)
    {
        if (EnableColor)
        {
            var color = level switch
            {
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Info => ConsoleColor.Blue,
                LogLevel.Warn => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Fatal => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };
            Console.ForegroundColor = color;
        }

        try
        {
            var finalMessage = string.Format(Format, DateTime.Now, level.ToString().ToUpper(), message);
            Console.WriteLine(finalMessage);
        }
        finally
        {
            if (EnableColor)
            {
                Console.ResetColor();
            }
        }
    }
}

internal class FileLogHandler(string logFolder) : ILogHandler
{
    private const string Format = "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}";
    private const string LogFileName = "log_{0:yyyyMMdd}.log";
    private const string ErrorFileName = "error_{0:yyyyMMdd}.log";
    private static readonly object Lock = new();

    public void Setup()
    {
        Directory.CreateDirectory(logFolder);
    }

    public void Teardown()
    {
    }

    public void DoLog(LogLevel level, string message)
    {
        lock (Lock)
        {
            var logFile = Path.Combine(logFolder, string.Format(LogFileName, DateTime.Now));
            var finalMessage = string.Format(Format, DateTime.Now, level.ToString().ToUpper(), message);
            try
            {
                using var sw = new StreamWriter(logFile, true, Encoding.UTF8);
                sw.WriteLine(finalMessage);

                if (level < LogLevel.Error) return;
                var errorFile = Path.Combine(logFolder, string.Format(ErrorFileName, DateTime.Now));
                using var swErr = new StreamWriter(errorFile, true, Encoding.UTF8);
                swErr.WriteLine(finalMessage);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
