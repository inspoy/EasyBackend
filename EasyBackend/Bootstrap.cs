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

        var appConfig = new AppConfig();
        var logger = new Logger();
        logger.Init(appConfig);
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
}
