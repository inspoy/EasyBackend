using EasyBackend.Utils;

namespace UnitTests;

[TestFixture]
public class TestCommon
{
    [SetUp]
    public void Setup()
    {
        Console.WriteLine("Start TestCommon");
    }

    [TearDown]
    public void TearDown()
    {
        Console.WriteLine("Finish TestCommon");
    }

    [Test]
    public void TestLaunchArgs()
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
        Assert.That(confPath, Is.EqualTo("config.json"));
        Assert.That(launchArgs.Get("port"), Is.EqualTo("8080"));
        Assert.That(launchArgs.Get("d"), Is.EqualTo("True"));
        Assert.IsNull(launchArgs.Get("debug"));
        Assert.DoesNotThrow(() => { launchArgs.Dump(); });
        Assert.DoesNotThrow(() => { launchArgs.HelpString(); });
    }

    [Test]
    public void TestLogging()
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
        var containModule = Does.Contain("TestModule");

        var output = Utils.GetOutput(() => { logger.Debug("Test Debug Message", "TestModule"); });
        Assert.That(output, Does.Contain("Debug"));
        Assert.That(output, containModule);
        Assert.That(output, Does.Contain("Test Debug Message"));
        output = Utils.GetOutput(() => { logger.Info("Test Info Message", "TestModule"); });
        Assert.That(output, Does.Contain("Info"));
        Assert.That(output, containModule);
        Assert.That(output, Does.Contain("Test Info Message"));
        output = Utils.GetOutput(() => { logger.Warn("Test Warn Message", "TestModule"); });
        Assert.That(output, Does.Contain("Warn"));
        Assert.That(output, containModule);
        Assert.That(output, Does.Contain("Test Warn Message"));
        output = Utils.GetOutput(() => { logger.Error("Test Error Message", "TestModule"); });
        Assert.That(output, Does.Contain("Error"));
        Assert.That(output, containModule);
        Assert.That(output, Does.Contain("Test Error Message"));
        output = Utils.GetOutput(() => { logger.Fatal("Test Fatal Message", "TestModule"); });
        Assert.That(output, Does.Contain("Fatal"));
        Assert.That(output, containModule);
        Assert.That(output, Does.Contain("Test Fatal Message"));
    }

    [Test]
    public void TestAppConfig()
    {
        var cfgPath = Utils.CreateTempConfig();
        var cfg = AppConfig.ReadFromFile(cfgPath);
        Assert.NotNull(cfg);
        Assert.That(cfg.Port, Is.EqualTo(8080));
        Assert.That(cfg.Logging.ConsoleColor, Is.True);
        Assert.That(cfg.RawYaml.otherField.field2, Is.EqualTo("value2"));
    }

    [Test]
    public void TestStartOption()
    {
        var cfgPath = Utils.CreateTempConfig();
        var cfg = AppConfig.ReadFromFile(cfgPath);
        var option = StartOption
            .CreateSimple(cfg)
            .WithPing()
            .WithReload(cfg?.Reload, null)
            .WithWelcome("Hello");
        Assert.That(option, Is.Not.Null);
    }
}
