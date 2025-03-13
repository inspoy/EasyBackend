using System.Text;
using EasyBackend.Http;

namespace EasyBackend.Routing;

public delegate Task RequestHandlerFunc(RequestWrapper req, ResponseWrapper res);

public class Router
{
    private readonly List<RequestHandler> _handlers = new();

    public RequestHandler AddHandler(string method, string pathPattern, RequestHandlerFunc handler, int priority = -1)
    {
        if (method.Contains('|'))
        {
            // multiple methods
            var methods = method.Split('|');
            foreach (var m in methods)
            {
                AddHandler(m, pathPattern, handler, priority);
            }

            return null;
        }

        if (priority < 0) priority = pathPattern.Length;
        var newOne = new RequestHandler(method, pathPattern, handler, priority);
        _handlers.Add(newOne);

        return newOne;
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

    public void Sort()
    {
        _handlers.Sort((a, b) => b.Priority - a.Priority);
        foreach (var handler in _handlers)
        {
            handler.Middlewares.Sort((a, b) => a.Sorting - b.Sorting);
        }
    }
}
