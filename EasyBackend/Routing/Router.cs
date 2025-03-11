using System.Text;
using EasyBackend.Http;

namespace EasyBackend.Routing;

public delegate Task RequestHandlerFunc(RequestWrapper req, ResponseWrapper res);

public class RequestHandler(string method, string pathPattern, RequestHandlerFunc handlerFunc, int priority)
{
    public string Method { get; } = method;
    public string PathPattern { get; } = pathPattern;
    public RequestHandlerFunc HandlerFunc { get; } = handlerFunc;
    public int Priority { get; } = priority;

    public bool TestPath(string path)
    {
        return PathPattern == path;
    }

    public async Task Execute(RequestWrapper req, ResponseWrapper res)
    {
        await HandlerFunc(req, res);
    }
}

public class Router
{
    private readonly List<RequestHandler> _handlers = new();

    public void AddHandler(string method, string pathPattern, RequestHandlerFunc handler, int priority = -1)
    {
        if (method.Contains('|'))
        {
            // multiple methods
            var methods = method.Split('|');
            foreach (var m in methods)
            {
                AddHandler(m, pathPattern, handler, priority);
            }

            return;
        }

        if (priority < 0) priority = pathPattern.Length;
        _handlers.Add(new RequestHandler(method, pathPattern, handler, priority));
        _handlers.Sort((a, b) => b.Priority - a.Priority);
    }

    public RequestHandler Match(string method, string path)
    {
        foreach (var item in _handlers)
        {
            if (item.TestPath(path)) return item;
        }

        return null;
    }

    public string Dump()
    {
        var sb = new StringBuilder();
        foreach (var item in _handlers)
        {
            sb.Append($"  [{item.Method}] {item.PathPattern}\n");
        }
        return sb.ToString();
    }
}
