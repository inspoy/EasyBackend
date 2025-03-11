using EasyBackend.Http;

namespace EasyBackend.Routing;

public class RequestHandler(string method, string pathPattern, RequestHandlerFunc handlerFunc, int priority)
{
    public string Method { get; } = method;
    public string PathPattern { get; } = pathPattern;
    public RequestHandlerFunc HandlerFunc { get; } = handlerFunc;
    public int Priority { get; } = priority;

    public List<IMiddleware> Middlewares { get; } = new();

    public bool TestPath(string path)
    {
        return PathPattern == path;
    }

    public async Task Execute(RequestWrapper req, ResponseWrapper res)
    {
        foreach (var middleware in Middlewares)
        {
            var result = middleware.PreExecute(req, res);
            if (!result) return;
        }

        await HandlerFunc(req, res);

        foreach (var middleware in Middlewares)
        {
            middleware.PostExecute(req, res);
        }
    }
}
