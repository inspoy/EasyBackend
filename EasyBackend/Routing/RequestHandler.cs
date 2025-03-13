using EasyBackend.Http;
using EasyBackend.Utils;

namespace EasyBackend.Routing;

public class RequestHandler(string method, string pathPattern, RequestHandlerFunc handlerFunc, int priority)
{
    private void OnAppConfigChanged()
    {
        foreach (var wrapper in _middlewares)
        {
            wrapper.ConfigAction(Instance.AppConfig, wrapper.Middleware);
        }
    }

    public string Method { get; } = method;
    public string PathPattern { get; } = pathPattern;
    public RequestHandlerFunc HandlerFunc { get; } = handlerFunc;
    public int Priority { get; } = priority;

    internal Bootstrap Instance
    {
        get => _instance;
        set
        {
            _instance = value;
            _instance.AppConfigChanged += OnAppConfigChanged;
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

    public bool TestPath(string path)
    {
        return PathPattern == path;
    }

    public async Task Execute(RequestWrapper req, ResponseWrapper res)
    {
        foreach (var wrapper in _middlewares)
        {
            var result = wrapper.Middleware.PreExecute(req, res);
            if (!result) return;
        }

        await HandlerFunc(req, res);

        foreach (var wrapper in _middlewares)
        {
            wrapper.Middleware.PostExecute(req, res);
        }
    }
}
