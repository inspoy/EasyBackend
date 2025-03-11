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
    public bool PreExecute(RequestWrapper req, ResponseWrapper res)
    {
        throw new NotImplementedException();
    }

    public void PostExecute(RequestWrapper req, ResponseWrapper res)
    {
        throw new NotImplementedException();
    }
}
