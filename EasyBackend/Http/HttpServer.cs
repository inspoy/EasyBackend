using System.Net;
using System.Text;
using EasyBackend.Routing;
using EasyBackend.Utils;

namespace EasyBackend.Http;

public delegate Task RequestHandler(RequestWrapper req, ResponseWrapper res);

public class HttpServer(AppConfig config, Logger logger)
{
    private HttpListener _listener;
    private Router _router = new();

    public void AddHandler(string method, string pathPattern, RequestHandler handler)
    {
        _router.AddHandler(method, pathPattern, handler);
    }

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
        var handler = _router.Match(reqWrapper.RawReq.HttpMethod, reqWrapper.RawReq.Url?.LocalPath);
        if (handler == null)
        {
            respWrapper.InitSimple(ResponseErrCode.NotFound, "No handler for this request");
        }
        else
        {
            await handler(reqWrapper, respWrapper);
        }

        logger.Debug("Response: " + respWrapper.BriefInfo, "Http");
        var resultJson = respWrapper.ToJson();
        res.StatusCode = (int)respWrapper.StatusCode;
        res.ContentType = "application/json; charset=utf-8";
        res.ContentEncoding = Encoding.UTF8;
        var buffer = Encoding.UTF8.GetBytes(resultJson);
        res.ContentLength64 = buffer.Length;
        await res.OutputStream.WriteAsync(buffer);
        res.Close();

        RequestWrapper.SaveReqId();
    }
}
