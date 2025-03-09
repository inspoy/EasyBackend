using EasyBackend;

Console.WriteLine("Hello, World!");
var bootstrap = Bootstrap.Create(args);
if (bootstrap == null)
{
    Console.WriteLine("Failed to create bootstrap");
    return;
}

bootstrap.Start();
var running = true;
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    bootstrap.Logger.Info("Ctrl+C detected, shutting down...");
    running = false;
};
while (running) Thread.Sleep(1000);
bootstrap.Shutdown();
