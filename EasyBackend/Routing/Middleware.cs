using System.Text;
using EasyBackend.Http;

namespace EasyBackend.Routing;

public interface IMiddleware
{
    /// <summary>
    /// 约小的优先级越高
    /// </summary>
    int Sorting { get; }
    /// <summary>
    /// 返回true表示执行成功，返回false表示执行失败应直接结束请求
    /// </summary>
    bool PreExecute(RequestWrapper req, ResponseWrapper res);

    void PostExecute(RequestWrapper req, ResponseWrapper res);
}

public class AuthMiddleWare : IMiddleware
{
    public int Sorting { get; } = 200;

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

public class ThrottleMiddleWare(int timeWindowMs, int reqLimit) : IMiddleware
{
    public int Sorting { get; } = 100;

    private readonly Dictionary<string, Queue<long>> _requestTimestamps = new();

    public bool PreExecute(RequestWrapper req, ResponseWrapper res)
    {
        var now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var ip = req.ClientIp;
        
        // 1. 确保队列
        if (!_requestTimestamps.TryGetValue(ip, out var timestamps))
        {
            timestamps = new Queue<long>();
            _requestTimestamps[ip] = timestamps;
        }

        // 2. 清理过期信息
        while (timestamps.Count > 0 && now - timestamps.Peek() > timeWindowMs)
        {
            timestamps.Dequeue();
        }

        // 3. 检查
        if (timestamps.Count >= reqLimit)
        {
            var first = timestamps.Peek();
            var waitTime = first + timeWindowMs - now;
            res.InitSimple(ResponseErrCode.TooManyRequests, $"Try again after {waitTime} ms");
            return false;
        }

        // 4. 记录
        timestamps.Enqueue(now);
        return true;
    }

    public void PostExecute(RequestWrapper req, ResponseWrapper res)
    {
    }
}
