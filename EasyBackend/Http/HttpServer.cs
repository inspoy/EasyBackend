using System.Diagnostics;
using System.Net;
using System.Text;
using EasyBackend.Routing;
using EasyBackend.Utils;

namespace EasyBackend.Http;

public class HttpServer(Bootstrap instance)
{
    private HttpListener _listener;
    private Router _router;

    public void Start(Router router)
    {
        _router = router;
        _router.Use(instance);
        var reqId = RequestWrapper.ResetReqId();
        instance.Logger.Info("RequestId reset to: " + reqId, "Http");
        _listener = new HttpListener();
        _listener.Prefixes.Add($"{instance.AppConfig.Host}:{instance.AppConfig.Port}/");
        if (!instance.MockMode)
            _listener.Start();
        instance.Logger.Info("Registered routes: \n" + _router.Dump(), "Http");
        instance.Logger.Info($"Listening on {instance.AppConfig.Host}:{instance.AppConfig.Port}", "Http");
        Receive();
    }

    public void Stop()
    {
        RequestWrapper.SaveReqId();
        _listener.Stop();
        _listener.Close();
        _router.UnUse();
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
        var respWrapper = new ResponseWrapper(reqWrapper.ReqId, res);
        var resultJson = await HandleImpl(reqWrapper, respWrapper);
        res.StatusCode = (int)respWrapper.StatusCode;
        res.ContentType = "application/json; charset=utf-8";
        res.ContentEncoding = Encoding.UTF8;
        var buffer = Encoding.UTF8.GetBytes(resultJson);
        res.ContentLength64 = buffer.Length;
        await res.OutputStream.WriteAsync(buffer);
        res.Close();

        RequestWrapper.SaveReqId();
    }

    private async Task<string> HandleImpl(RequestWrapper reqWrapper, ResponseWrapper respWrapper)
    {
        instance.Logger.Debug("-> " + reqWrapper.BriefInfo, "Http");
        var sw = Stopwatch.StartNew();
        var handler = _router.Match(reqWrapper);
        if (handler == null)
        {
            respWrapper.InitSimple(ResponseErrCode.NotFound, "No handler for this request");
        }
        else
        {
            instance.Logger.Debug("Handle with " + handler.PathPattern, "Http");
            await handler.Execute(reqWrapper, respWrapper);
        }

        var timeCost = sw.ElapsedMilliseconds;
        instance.Logger.Debug("<- " + respWrapper.BriefInfo + $"|cost {timeCost:N0}ms", "Http");
        var resultJson = respWrapper.ToJson();
        return resultJson;
    }

    // Only for testing purposes
    internal string TestHandle(MockContext ctx)
    {
        var reqWrapper = new RequestWrapper(ctx);
        var respWrapper = new ResponseWrapper(reqWrapper.ReqId, ctx);
        var task = HandleImpl(reqWrapper, respWrapper);
        task.Wait();
        return task.Result;
    }
}
