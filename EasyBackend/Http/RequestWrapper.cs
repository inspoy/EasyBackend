using System.Net;

namespace EasyBackend.Http;

public partial class RequestWrapper(HttpListenerRequest rawReq)
{
    public HttpListenerRequest RawReq => rawReq;
    public ulong ReqId { get; } = _reqId++;
}
