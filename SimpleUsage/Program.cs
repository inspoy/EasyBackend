using EasyBackend;
using EasyBackend.Http;
using EasyBackend.Routing;
using EasyBackend.Utils;

Console.WriteLine("Hello, World!");
var bootstrap = Bootstrap.Create(args);
if (bootstrap == null)
{
    Console.WriteLine("Failed to create bootstrap");
    return;
}

var option = StartOption
    .CreateSimple(bootstrap.AppConfig)
    .WithWelcome("Hello from EasyBackend")
    .WithReload(bootstrap.AppConfig.Reload, bootstrap.ReloadConfig);
var pingHandler = option.Router.AddHandler("GET", "/ping", (req, res) =>
{
    res.InitSimple(ResponseErrCode.Success, "pong");
    return Task.CompletedTask;
});
pingHandler.AddMiddleware(
    new SimpleUserManagerMiddleware([
        new()
        {
            Token = "123456",
            TimeWindowMs = 5000,
            ReqLimit = 3
        }
    ])
);
bootstrap.StartDaemon(option);
