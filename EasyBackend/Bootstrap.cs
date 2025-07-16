using System.Runtime.CompilerServices;
using EasyBackend.Http;
using EasyBackend.Utils;

[assembly: InternalsVisibleTo("UnitTests")]

namespace EasyBackend;

public class Bootstrap
{
    public static Bootstrap Current { get; private set; }

    public static Bootstrap Create(string[] args)
    {
        var launchArgs = new LaunchArgs(args);
        var argBook = new List<LaunchArgItem>
        {
            new("config", 'c', "Path to config file", "appConf.yml")
        };
        launchArgs.ApplyBook(argBook);
        var confPath = launchArgs.Get("config");

        var appConfig = AppConfig.ReadFromFile(confPath);
        if (appConfig == null)
        {
            return null;
        }

        var logger = new Logger();
        logger.Init(appConfig.Logging);
        logger.Info(launchArgs.Dump());

        var bootstrap = new Bootstrap
        {
            Logger = logger,
            LaunchArgs = launchArgs,
            AppConfig = appConfig,
            _configPath = confPath
        };
        Current = bootstrap;
        return bootstrap;
    }

    internal bool MockMode { get; set; } = false;
    internal LaunchArgs LaunchArgs { get; private set; }
    public Logger Logger { get; private set; }
    public AppConfig AppConfig { get; private set; }
    public event Action AppConfigChanged;

    private string _configPath;
    private HttpServer _httpServer;

    public void StartDaemon(StartOption option)
    {
        Start(option);
        var running = true;
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            Logger.Info("Ctrl+C detected, shutting down...", "Bootstrap");
            running = false;
        };
        while (running) Thread.Sleep(1000);
        try
        {
            foreach (var action in option.ShutdownActions)
            {
                action?.Invoke();
            }
        }
        catch (Exception e)
        {
            Logger.Error("Error during shutdown action: " + e.Message, "Bootstrap");
        }

        Shutdown();
    }

    public void Start(StartOption option)
    {
        Logger.Info("Starting...", "Bootstrap");
        _httpServer = new HttpServer(this);
        _httpServer.Start(option.Router);
        Logger.Info("Started", "Bootstrap");
    }

    public void Shutdown()
    {
        Logger.Info("Shutting down...", "Bootstrap");
        Logger.Cleanup();
        _httpServer.Stop();
        _httpServer = null;
    }

    public void ReloadConfig()
    {
        var appConfig = AppConfig.ReadFromFile(_configPath);
        if (appConfig != null)
        {
            Logger.Info("Config reloaded", "Bootstrap");
            AppConfig = appConfig;
            AppConfigChanged?.Invoke();
            Logger.Info("New config has notified to handlers", "Bootstrap");
        }
    }
}
