using System.Reflection;
using EasyBackend;
using EasyBackend.Http;
using EasyBackend.Routing;
using EasyBackend.Utils;

namespace UnitTests;

[TestFixture]
public class TestBoostrap
{
    [Test]
    public void TestLifeCycle()
    {
        var bootstrap = PrepareBootstrap(out _);

        // Assert
        Assert.That(bootstrap.Logger, Is.Not.Null, "Logger should be initialized");
        Assert.That(bootstrap.AppConfig, Is.Not.Null, "AppConfig should be initialized");

        // Check if the logger is set up correctly
        Assert.DoesNotThrow(() => bootstrap.Logger.Debug("Bootstrap initialized successfully"));

        bootstrap.Shutdown();
    }

    [Test]
    public void TestHandle()
    {
        var bootstrap = PrepareBootstrap(out var httpServer);

        var mockCtx1 = Utils.CreateMockContext("GET", "/");
        var result1 = httpServer.TestHandle(mockCtx1);
        Assert.That(result1, Does.Contain("Hello"));

        var mockCtx2 = Utils.CreateMockContext("GET", "/ping");
        var result2 = httpServer.TestHandle(mockCtx2);
        Assert.That(result2, Does.Contain("pong"));
        
        var mockCtx3 = Utils.CreateMockContext("OPTION", "/");
        var result3 = httpServer.TestHandle(mockCtx3);
        Assert.That(result3, Does.Contain("OK"));
        bootstrap.Shutdown();
    }

    [Test]
    public void TestReloadConfig()
    {
        var bootstrap = PrepareBootstrap(out var httpServer);

        bootstrap.ReloadConfig();

        var mockCtx = Utils.CreateMockContext("POST", "/reload_config");
        var result = httpServer.TestHandle(mockCtx);
        Assert.That(result, Does.Contain("Invalid Authorization header"));

        var mockCtx2 = Utils.CreateMockContext("POST", "/reload_config");
        mockCtx2.RequestHeaders.Add("Authorization", "Bearer 123");
        result = httpServer.TestHandle(mockCtx2);
        Assert.That(result, Does.Contain("Config reloaded"));

        bootstrap.Shutdown();
    }

    [Test]
    public void TestPathMatching()
    {
        var bootstrap = PrepareBootstrap(out var httpServer, router =>
        {
            router.AddHandler("GET", "/user/{id}", (req, res) =>
            {
                res.InitSimple(ResponseErrCode.Success, "PathA|" + req.GetPathParam("id"));
                return Task.CompletedTask;
            });
            router.AddHandler("GET", "/user/*", (req, res) =>
            {
                res.InitSimple(ResponseErrCode.Success, "PathB|" + req.GetPathParam("wildcard"));
                return Task.CompletedTask;
            });
            router.AddHandler("GET", "/user/{id}/*", (req, res) =>
            {
                res.InitSimple(ResponseErrCode.Success,
                    "PathC|" + req.GetPathParam("id") + "|" + req.GetPathParam("wildcard"));
                return Task.CompletedTask;
            });
        });

        string result;
        result = httpServer.TestHandle(Utils.CreateMockContext("GET", "/user/123"));
        Assert.That(result, Does.Contain("PathA"));
        Assert.That(result, Does.Contain("123"));
        result = httpServer.TestHandle(Utils.CreateMockContext("GET", "/user/abc"));
        Assert.That(result, Does.Contain("PathA"));
        Assert.That(result, Does.Contain("abc"));
        result = httpServer.TestHandle(Utils.CreateMockContext("GET", "/user/123/abc"));
        Assert.That(result, Does.Contain("PathC"));
        Assert.That(result, Does.Contain("123"));
        Assert.That(result, Does.Contain("abc"));
        result = httpServer.TestHandle(Utils.CreateMockContext("GET", "/user/"));
        Assert.That(result, Does.Contain("No handler"));
        result = httpServer.TestHandle(Utils.CreateMockContext("GET", "/account/123"));
        Assert.That(result, Does.Contain("No handler"));
    }

    [Test]
    public static void TestUserManager()
    {
        var bootstrap = PrepareBootstrap(out var httpServer, router =>
        {
            var userManager = new SimpleUserManagerMiddleware(new List<SimpleUserManagerMiddleware.UserProfile>
            {
                new() { Token = "123", TimeWindowMs = 1000, ReqLimit = 10 },
            });
            router.AddHandler("GET", "/user", (req, res) =>
            {
                res.InitSimple(ResponseErrCode.Success, "User accessed");
                return Task.CompletedTask;
            }).AddMiddleware(userManager);
        });
        var ctx = Utils.CreateMockContext("GET", "/user");
        var result = httpServer.TestHandle(ctx);
        Assert.That(result, Does.Contain("Invalid Authorization header"));
        ctx.RequestHeaders.Add("Authorization", "Bearer 123");
        result = httpServer.TestHandle(ctx);
        Assert.That(result, Does.Contain("User accessed"));
        bootstrap.Shutdown();
    }

    private static Bootstrap PrepareBootstrap(out HttpServer httpServer, Action<Router> routerInit = null)
    {
        var cfgPath = Utils.CreateTempConfig();
        var cfg = AppConfig.ReadFromFile(cfgPath);
        var option = StartOption
            .CreateSimple(cfg)
            .WithPing()
            .WithReload(cfg?.Reload, null)
            .WithWelcome("Hello")
            .AddShutdownAction(() => { });
        routerInit?.Invoke(option.Router);

        var bootstrap = Bootstrap.Create(["-c", cfgPath]);
        Assert.That(bootstrap, Is.Not.Null, "Bootstrap should be created successfully");
        bootstrap.MockMode = true;
        bootstrap.Start(option);

        var fi = typeof(Bootstrap).GetField("_httpServer", BindingFlags.Instance | BindingFlags.NonPublic);
        httpServer = (HttpServer)fi?.GetValue(bootstrap);
        Assert.That(httpServer, Is.Not.Null, "HttpServer should be initialized");

        return bootstrap;
    }
}
