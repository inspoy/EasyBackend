using EasyBackend;
using EasyBackend.Http;
using EasyBackend.Utils;

Console.WriteLine("Hello, World!");
var bootstrap = Bootstrap.Create(args);
if (bootstrap == null)
{
    Console.WriteLine("Failed to create bootstrap");
    return;
}

var option = StartOption.CreateSimple();
option.Router.AddHandler("GET", "/ping", (req, res) =>
{
    res.InitSimple(ResponseErrCode.Success, "pong");
    return Task.CompletedTask;
});
bootstrap.Start(option);
var running = true;
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    bootstrap.Logger.Info("Ctrl+C detected, shutting down...");
    running = false;
};
while (running) Thread.Sleep(1000);
bootstrap.Shutdown();
