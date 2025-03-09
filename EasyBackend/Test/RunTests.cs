using EasyBackend.Utils;

namespace EasyBackend.Test;

public static class RunTests
{
    public static void TestLaunchArgs()
    {
        var strArgs = "-c config.json --port 8080 -d";
        var args = strArgs.Split(' ');
        var launchArgs = new LaunchArgs(args);
        launchArgs.Check(new Dictionary<char, string>
        {
            { 'c', "config" }
        });
        var confPath = launchArgs.Get("config");
        if (confPath != "config.json")
        {
            throw new Exception("TestLaunchArgs failed");
        }
    }
}
