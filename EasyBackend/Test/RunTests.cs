using EasyBackend.Utils;

namespace EasyBackend.Test;

public static class RunTests
{
    public static void Run()
    {
        TestLaunchArgs();
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
}
