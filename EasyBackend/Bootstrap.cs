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
            return null;
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
            AppConfig = appConfig
        };
        return bootstrap;
    }

    internal LaunchArgs LaunchArgs { get; private set; }
    public Logger Logger { get; private set; }
    public AppConfig AppConfig { get; private set; }

    public void Start()
    {
        Logger.Info("Starting...", "Bootstrap");
        Logger.Info("Started", "Bootstrap");
    }

    public void Shutdown()
    {
        Logger.Info("Shutting down...", "Bootstrap");
    }
}
