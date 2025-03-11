using EasyBackend.Http;

namespace EasyBackend.Routing;

public class Handler
{
    public string Method { get; }
    public string PathPattern { get; }
    public RequestHandler HandlerFunc { get; }

    public Handler(string method, string pathPattern, RequestHandler handlerFunc)
    {
        Method = method;
        PathPattern = pathPattern;
        HandlerFunc = handlerFunc;
    }

    public bool TestPath(string path)
    {
        return PathPattern == path;
    }
}

public class Router
{
    private List<Handler> _handlers = new();

    public void AddHandler(string method, string pathPattern, RequestHandler handler)
    {
        _handlers.Add(new Handler(method, pathPattern, handler));
    }

    public RequestHandler Match(string method, string path)
    {
        foreach (var item in _handlers)
        {
            if (item.TestPath(path)) return item.HandlerFunc;
        }

        return null;
    }
}
