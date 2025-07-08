using EasyBackend.Http;

namespace EasyBackend.Routing;

public abstract class PathRouter
{
    public virtual RequestHandlerFunc OnGet { get; } = null;
    public virtual RequestHandlerFunc OnPost { get; } = null;
    public virtual RequestHandlerFunc OnPut { get; } = null;
    public virtual RequestHandlerFunc OnPatch { get; } = null;
    public virtual RequestHandlerFunc OnDelete { get; } = null;
}
