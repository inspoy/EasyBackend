using EasyBackend.Http;
using EasyBackend.Routing;
using EasyBackend.Utils;

namespace EasyBackend.Test;

public static class RunTests
{
    public static void Run()
    {
        TestLaunchArgs();
        TestLogging();
        TestResponseWrapper();
        TestThrottleMiddleware();
    }

    private static void TestThrottleMiddleware()
    {
        var throttle = new ThrottleMiddleware(2000, 5);
        for (var i = 0; i < 10; ++i)
        {
            var req = new RequestWrapper(null);
            var res = new ResponseWrapper(req.ReqId, null);
            var result = throttle.PreExecute(req, res);
            Console.WriteLine($"{res.ReqId} - {result} - {res.ToJson()}");
            Thread.Sleep(300);
        }
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
        launchArgs.ApplyBook(argBook);
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
        var resp = new ResponseWrapper(12345, null);
        resp.InitSimple(ResponseErrCode.NotImplement, "Only for testing");
        Console.WriteLine(resp.BriefInfo);
        Console.WriteLine(resp.ToJson());
    }
}
