using System.Text;
using EasyBackend.Http;

namespace EasyBackend.Routing;

public interface IMiddleware
{
    /// <summary>
    /// 返回true表示执行成功，返回false表示执行失败应直接结束请求
    /// </summary>
    bool PreExecute(RequestWrapper req, ResponseWrapper res);

    void PostExecute(RequestWrapper req, ResponseWrapper res);
}

public class AuthMiddleWare : IMiddleware
{
    private readonly string _authString;

    public AuthMiddleWare(string bearerToken)
    {
        _authString = "Bearer " + bearerToken;
    }

    public AuthMiddleWare(string userName, string password)
    {
        _authString = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
    }

    public bool PreExecute(RequestWrapper req, ResponseWrapper res)
    {
        var authHeader = req.RawReq.Headers.Get("Authorization");
        if (string.IsNullOrEmpty(authHeader))
        {
            res.InitSimple(ResponseErrCode.InvalidToken, "Authorization header is missing");
            return false;
        }

        if (authHeader != _authString)
        {
            res.InitSimple(ResponseErrCode.InvalidToken, "Valid token is required");
            return false;
        }

        return true;
    }

    public void PostExecute(RequestWrapper req, ResponseWrapper res)
    {
    }
}

public class ThrottleMiddleWare : IMiddleware
{
    public ulong IntervalMs { get; set; }

    private readonly Dictionary<string, ulong> _lastReqTime = new();

    public bool PreExecute(RequestWrapper req, ResponseWrapper res)
    {
        var now = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
        var ip = req.ClientIp;
        if (_lastReqTime.TryGetValue(ip, out var lastReqTime)
            && now - lastReqTime < IntervalMs)
        {
            res.InitSimple(ResponseErrCode.TooManyRequests, "Too many requests");
            return false;
        }

        _lastReqTime[ip] = now;
        return true;
    }

    public void PostExecute(RequestWrapper req, ResponseWrapper res)
    {
    }
}
