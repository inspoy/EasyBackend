using System.Text;
using EasyBackend.Http;
using EasyBackend.Utils;

namespace EasyBackend.Routing;

internal class MiddlewareWrapper : IComparable<MiddlewareWrapper>
{
    public IMiddleware Middleware { get; init; }
    public Action<AppConfig, IMiddleware> ConfigAction { get; init; }

    public int CompareTo(MiddlewareWrapper other)
    {
        return Middleware.Sorting - other.Middleware.Sorting;
    }
}

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

public class AuthMiddleware() : IMiddleware
{
    public int Sorting { get; } = 200;
    public string AuthType { get; set; }
    public string AuthValue { get; set; }

    public AuthMiddleware(string bearerToken) : this()
    {
        AuthType = "Bearer";
        AuthValue = bearerToken;
    }

    public AuthMiddleware(string userName, string password) : this()
    {
        AuthType = "Basic";
        AuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
    }

    public (string authType, string authValue) SplitAuthHeader(RequestWrapper req)
    {
        var authHeader = req.RawReq.Headers.Get("Authorization");
        if (string.IsNullOrEmpty(authHeader))
        {
            return (null, null);
        }

        var parts = authHeader.Split(' ');
        if (parts.Length != 2)
        {
            return (null, null);
        }

        return (parts[0], parts[1]);
    }

    public bool PreExecute(RequestWrapper req, ResponseWrapper res)
    {
        var (authType, authValue) = SplitAuthHeader(req);
        if (string.IsNullOrEmpty(authType) || string.IsNullOrEmpty(authValue))
        {
            res.InitSimple(ResponseErrCode.InvalidToken, "Invalid Authorization header");
            return false;
        }

        if (authType != AuthType || authValue != AuthValue)
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

public class ThrottleMiddleware(int timeWindowMs, int reqLimit) : IMiddleware
{
    public int Sorting { get; } = 100;
    public int TimeWindowMs { get; set; } = timeWindowMs;
    public int ReqLimit { get; set; } = reqLimit;

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
        while (timestamps.Count > 0 && now - timestamps.Peek() > TimeWindowMs)
        {
            timestamps.Dequeue();
        }

        // 3. 检查
        if (timestamps.Count >= ReqLimit)
        {
            var first = timestamps.Peek();
            var waitTime = first + TimeWindowMs - now;
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

public class SimpleUserManagerMiddleware() : IMiddleware
{
    public class UserProfile
    {
        public string Token;
        public int TimeWindowMs;
        public int ReqLimit;
    }

    public int Sorting { get; } = 300;

    private readonly Dictionary<string, UserProfile> _users = new();
    private readonly AuthMiddleware _auth = new();
    private readonly ThrottleMiddleware _throttle = new(0, 0);

    public SimpleUserManagerMiddleware(List<UserProfile> users) : this()
    {
        foreach (var user in users)
        {
            _users[user.Token] = user;
        }
    }

    public bool PreExecute(RequestWrapper req, ResponseWrapper res)
    {
        var (authType, authValue) = _auth.SplitAuthHeader(req);
        if (string.IsNullOrEmpty(authType) || string.IsNullOrEmpty(authValue) || authType != "Bearer")
        {
            res.InitSimple(ResponseErrCode.InvalidToken, "Invalid Authorization header");
            return false;
        }

        if (!_users.TryGetValue(authValue, out var user))
        {
            res.InitSimple(ResponseErrCode.InvalidToken, "Invalid token");
            return false;
        }

        if (user.TimeWindowMs <= 0 || user.ReqLimit <= 0)
        {
            // 没有流控
            return true;
        }

        _throttle.TimeWindowMs = user.TimeWindowMs;
        _throttle.ReqLimit = user.ReqLimit;
        return _throttle.PreExecute(req, res);
    }

    public void PostExecute(RequestWrapper req, ResponseWrapper res)
    {
    }
}
