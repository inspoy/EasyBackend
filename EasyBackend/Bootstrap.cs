using EasyBackend.Http;
using EasyBackend.Utils;

namespace EasyBackend;

public class Bootstrap
{
    public static Bootstrap Create(string[] args)
    {
        var launchArgs = new LaunchArgs(args);
        var argBook = new List<LaunchArgItem>
        {
            new("config", 'c', "Path to config file")
        };
        launchArgs.Check(argBook);
        var confPath = launchArgs.Get("config");
        if (confPath == null)
        {
            confPath = "appConf.yml";
        }

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
        return bootstrap;
    }

    internal LaunchArgs LaunchArgs { get; private set; }
    public Logger Logger { get; private set; }
    public AppConfig AppConfig { get; private set; }

    private string _configPath;
    private HttpServer _httpServer;

    public void Start()
    {
        Logger.Info("Starting...", "Bootstrap");
        _httpServer = new HttpServer(AppConfig, Logger);
        _httpServer.Start();
        Logger.Info("Started", "Bootstrap");
    }

    public void Shutdown()
    {
        Logger.Info("Shutting down...", "Bootstrap");
        _httpServer.Stop();
        _httpServer = null;
    }

    public void ReloadConfig()
    {
        var appConfig = AppConfig.ReadFromFile(_configPath);
        if (appConfig != null)
        {
            AppConfig = appConfig;
        }
    }
}
