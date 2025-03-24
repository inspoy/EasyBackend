using System.Text;
using EasyBackend.Http;
using EasyBackend.Utils;

namespace EasyBackend.Routing;

public delegate Task RequestHandlerFunc(RequestWrapper req, ResponseWrapper res);

public delegate void HandlerReConfigFunc(AppConfig conf, RequestHandler handler);

public class Router
{
    private readonly List<RequestHandler> _handlers = new();
    private bool _inUse;

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

    /// <summary>
    /// 匹配请求对应的处理器
    /// </summary>
    /// <param name="req">请求包装器</param>
    /// <returns>匹配成功返回对应的请求处理器，否则返回null</returns>
    /// <remarks>
    /// 该方法遍历所有注册的处理器，根据HTTP方法和URL路径进行匹配。
    /// 当找到匹配的处理器时，会设置路径参数并返回该处理器。
    /// </remarks>
    public RequestHandler Match(RequestWrapper req)
    {
        var method = req.Method;
        var path = req.Url.LocalPath;
        foreach (var item in _handlers)
        {
            if (item.Method == method && item.TestPath(path, out var pathParams))
            {
                item.SetPathParams(pathParams);
                return item;
            }
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

    private void Sort()
    {
        _handlers.Sort((a, b) => b.Priority - a.Priority);
        foreach (var handler in _handlers)
        {
            handler.SortMiddlewares();
        }
    }

    /// <summary>
    /// 开始使用，绑定Bootstrap
    /// </summary>
    /// <exception cref="InvalidOperationException">已经用其他Bootstrap设置过了</exception>
    internal void Use(Bootstrap instance)
    {
        if (_inUse)
        {
            throw new InvalidOperationException("Router already in use");
        }

        _inUse = true;
        Sort();
        foreach (var handler in _handlers)
        {
            handler.Instance = instance;
        }
    }

    internal void UnUse()
    {
        _inUse = false;
        foreach (var handler in _handlers)
        {
            handler.Instance = null;
        }
    }
}
