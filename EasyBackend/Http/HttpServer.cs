using System.Net;
using System.Text;
using EasyBackend.Utils;

namespace EasyBackend.Http;

public class HttpServer(AppConfig config, Logger logger)
{
    private HttpListener _listener;

    public void Start()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"{config.Host}:{config.Port}/");
        _listener.Start();
        logger.Info($"Listening on {config.Host}:{config.Port}", "Http");
        Receive();
    }

    public void Stop()
    {
        _listener.Stop();
        _listener.Close();
    }
    
    private async void Receive()
    {
        while (_listener.IsListening)
        {
            var ctx = await _listener.GetContextAsync();
            var req = ctx.Request;
            var res = ctx.Response;
            await HandleRequest(req, res);
            res.Close();
        }
    }

    private async Task HandleRequest(HttpListenerRequest req, HttpListenerResponse resp)
    {
        logger.Debug("Handle request", "Http");
        await Task.Delay(1000);
        resp.StatusCode = 200;
        resp.ContentType = "text/plain";
        resp.ContentEncoding = Encoding.UTF8;
        var buffer = Encoding.UTF8.GetBytes("Hello, World!");
        resp.ContentLength64 = buffer.Length;
        await resp.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    }
}
