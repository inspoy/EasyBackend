using EasyBackend.Utils;

namespace EasyBackend;

public class Bootstrap
{
    public static Bootstrap Create(string[] args)
    {
        var launchArgs = new LaunchArgs(args);
        launchArgs.Check(new Dictionary<char, string>
        {
            { 'c', "config" }
        });
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

    public Logger Logger { get; private set; }
    internal LaunchArgs LaunchArgs { get; private set; }
    internal AppConfig AppConfig { get; private set; }
}
