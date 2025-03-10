using System.Net;
using System.Text;
using EasyBackend.Utils;

namespace EasyBackend.Http;

public class HttpServer(AppConfig config, Logger logger)
{
    private HttpListener _listener;

    public void Start()
    {
        var reqId = RequestWrapper.ResetReqId();
        logger.Info("RequestId reset to: " + reqId, "Http");
        _listener = new HttpListener();
        _listener.Prefixes.Add($"{config.Host}:{config.Port}/");
        _listener.Start();
        logger.Info($"Listening on {config.Host}:{config.Port}", "Http");
        Receive();
    }

    public void Stop()
    {
        RequestWrapper.SaveReqId();
        _listener.Stop();
        _listener.Close();
    }

    private async void Receive()
    {
        while (_listener.IsListening)
        {
            var ctx = await _listener.GetContextAsync();
            _ = Task.Run(() => HandleRequest(ctx));
        }
    }

    private async Task HandleRequest(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var res = ctx.Response;
        var reqWrapper = new RequestWrapper(req);
        logger.Debug("Request : " + reqWrapper.BriefInfo, "Http");
        var respWrapper = new ResponseWrapper(reqWrapper.ReqId);
        respWrapper.InitSimple(ResponseErrCode.Success, "Hello, World!");
        await Task.Delay(3000);
        logger.Debug("Response: " + respWrapper.BriefInfo, "Http");
        var resultJson = respWrapper.ToJson();
        res.StatusCode = (int)respWrapper.StatusCode;
        res.ContentType = "application/json; charset=utf-8";
        res.ContentEncoding = Encoding.UTF8;
        var buffer = Encoding.UTF8.GetBytes(resultJson);
        res.ContentLength64 = buffer.Length;
        await res.OutputStream.WriteAsync(buffer);
        res.Close();
    }
}
