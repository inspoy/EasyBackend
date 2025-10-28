using EasyBackend.Http;
using EasyBackend.Utils;

namespace EasyBackend.Routing;

public class RequestHandler(string method, string pathPattern, RequestHandlerFunc handlerFunc, int priority)
{
    private void OnAppConfigChanged()
    {
        ReConfigFunc?.Invoke(Instance.AppConfig, this);
        foreach (var wrapper in _middlewares)
        {
            wrapper.ConfigAction(Instance.AppConfig, wrapper.Middleware);
        }
    }

    public string Method { get; } = method;
    public string PathPattern { get; } = pathPattern;
    public RequestHandlerFunc HandlerFunc { get; } = handlerFunc;

    /// <summary>
    /// 优先级越大的越先尝试执行
    /// </summary>
    public int Priority { get; } = priority;

    public HandlerReConfigFunc ReConfigFunc { get; set; }


    internal Bootstrap Instance
    {
        get => _instance;
        set
        {
            if (value == null && _instance != null)
            {
                _instance.AppConfigChanged -= OnAppConfigChanged;
            }

            _instance = value;
            if (_instance == null) return;
            _instance.AppConfigChanged += OnAppConfigChanged;
            OnAppConfigChanged();
        }
    }

    private readonly List<MiddlewareWrapper> _middlewares = new();
    private Bootstrap _instance;

    public void AddMiddleware<T>(T middleware, Action<AppConfig, T> configAction = null) where T : class, IMiddleware
    {
        _middlewares.Add(new MiddlewareWrapper
        {
            Middleware = middleware,
            ConfigAction = (conf, m) => { configAction?.Invoke(conf, m as T); }
        });
    }

    public void SortMiddlewares()
    {
        _middlewares.Sort();
    }

    public bool TestPath(string path, out Dictionary<string, string> pathParams)
    {
        pathParams = null;
        // 完全相等的情况
        if (PathPattern == path) return true;

        // 完全通配符的情况
        if (PathPattern == "*")
        {
            pathParams ??= new();
            pathParams.Add("wildcard", path);
            return true;
        }

        var pattern = PathPattern.Split('/');
        var pathParts = path.TrimEnd('/').Split('/');

        // 处理通配符情况
        if (PathPattern.EndsWith("/*"))
        {
            // 去掉通配符的部分
            var basePathParts = PathPattern.Substring(0, PathPattern.Length - 2).Split('/');

            // 判断基本路径分段数量
            if (pathParts.Length <= basePathParts.Length) return false;

            // 检查除了通配符外的基本路径是否匹配
            for (var i = 0; i < basePathParts.Length; i++)
            {
                if (basePathParts[i] == pathParts[i]) continue;
                if (basePathParts[i].StartsWith("{") && basePathParts[i].EndsWith("}"))
                {
                    // 提取参数
                    var paramName = basePathParts[i].Substring(1, basePathParts[i].Length - 2);
                    pathParams ??= new();
                    pathParams.Add(paramName, pathParts[i]);
                    continue;
                }

                return false;
            }

            // 将剩余路径作为通配符参数
            var wildcardPath = string.Join("/", pathParts.Skip(basePathParts.Length));
            pathParams ??= new();
            pathParams.Add("wildcard", wildcardPath);
            return true;
        }

        // 参数化路径处理 (包含 {} 的路径但没有通配符)
        if (PathPattern.Contains("{") && PathPattern.Contains("}"))
        {
            if (pattern.Length != pathParts.Length) return false;

            for (var i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == pathParts[i]) continue;
                if (pattern[i].StartsWith("{") && pattern[i].EndsWith("}"))
                {
                    // 提取参数
                    var paramName = pattern[i].Substring(1, pattern[i].Length - 2);
                    pathParams ??= new();
                    pathParams.Add(paramName, pathParts[i]);
                    continue;
                }

                return false;
            }

            return true;
        }

        return false;
    }

    public async Task Execute(RequestWrapper req, ResponseWrapper res)
    {
        foreach (var wrapper in _middlewares)
        {
            var result = wrapper.Middleware.PreExecute(req, res);
            if (!result)
            {
                res.InitSimple(ResponseErrCode.InvalidRequest, "Request blocked by middleware.");
                return;
            }
        }

        try
        {
            await HandlerFunc(req, res);
        }
        catch (Exception ex)
        {
            _instance.Logger.Error($"Error executing request handler for {Method} {PathPattern}: {ex}");
            res.InitSimple(ResponseErrCode.ServerError, "Internal server error.");
        }

        foreach (var wrapper in _middlewares)
        {
            wrapper.Middleware.PostExecute(req, res);
        }
    }
}
