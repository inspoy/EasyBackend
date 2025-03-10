using EasyBackend.Http;
using EasyBackend.Utils;

namespace EasyBackend.Test;

public static class RunTests
{
    public static void Run()
    {
        TestLaunchArgs();
        TestLogging();
        TestResponseWrapper();
    }

    private static void TestLaunchArgs()
    {
        var strArgs = "-c config.json -p 8080 -d";
        var args = strArgs.Split(' ');
        var launchArgs = new LaunchArgs(args);
        var argBook = new List<LaunchArgItem>
        {
            new("config", 'c', "Path to config file"),
            new("port", 'p', "Port to listen"),
            new("debug", "Run in debug mode")
        };
        launchArgs.Check(argBook);
        var confPath = launchArgs.Get("config");
        Console.WriteLine(confPath);
        Console.WriteLine(launchArgs.Dump());
        Console.WriteLine(launchArgs.HelpString());
    }

    private static void TestLogging()
    {
        if (!Directory.Exists("./logs"))
        {
            Directory.CreateDirectory("./logs");
        }

        var logger = new Logger();
        logger.Init(new AppConfigLogging
        {
            ConsoleEnabled = true,
            ConsoleColor = true,
            LogFileFolder = "./logs"
        });

        logger.Debug("Test Debug Message");
        logger.Info("Test Info Message");
        logger.Warn("Test Warn Message");
        logger.Error("Test Error Message");
        logger.Fatal("Test Fatal Message");
    }
    
    public static void TestResponseWrapper()
    {
        var resp = new ResponseWrapper(12345);
        resp.InitSimple(ResponseErrCode.NotImplement, "Only for testing");
        Console.WriteLine(resp.BriefInfo);
        Console.WriteLine(resp.ToJson());
    }
}
