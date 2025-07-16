using System.Net;
using JetBrains.Annotations;

namespace EasyBackend.Http;

public partial class RequestWrapper
{
    public ulong ReqId { get; } = Interlocked.Increment(ref _reqId);
    public string Method => _rawReq?.HttpMethod ?? _mock.Method;
    public Uri Url => _rawReq?.Url ?? _mock?.Url;

    public string BriefInfo =>
        $"{ReqId}-[{Method}] {Url.LocalPath} from {ClientIp}, q={Query.Count}, b={Body.Length}";

    public string ClientIp =>
        _clientIp ??= GetHeader("X-Real-Ip") ??
                      GetHeader("Remote_Ip") ??
                      _rawReq?.RemoteEndPoint?.Address.ToString() ??
                      _mock?.RemoteAddress ??
                      "unknown";

    public Dictionary<string, string> Query
    {
        get
        {
            if (_queryDict != null) return _queryDict;

            var query = Url.Query;
            _queryDict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(query)) return _queryDict;
            var pairs = query.TrimStart('?').Split('&');
            foreach (var pair in pairs)
            {
                if (string.IsNullOrEmpty(pair)) continue;
                var idx = pair.IndexOf('=');
                if (idx < 0)
                {
                    _queryDict[pair] = "";
                }

                var k = pair.Substring(0, idx);
                var v = pair.Substring(idx + 1);
                v = WebUtility.UrlDecode(v);
                _queryDict[k] = v;
            }

            return _queryDict;
        }
    }

    public string Body
    {
        get
        {
            if (_body != null) return _body;
            if (_rawReq != null)
                _body = new StreamReader(_rawReq.InputStream).ReadToEnd();
            else if (_mock != null)
                _body = _mock.Body;
            return _body;
        }
    }

    private string _body;
    private string _clientIp;
    private Dictionary<string, string> _queryDict;
    private readonly HttpListenerRequest _rawReq;
    private readonly MockContext _mock;
    private Dictionary<string, string> _pathParams;

    public RequestWrapper(HttpListenerRequest rawReq)
    {
        _rawReq = rawReq;
    }

    internal RequestWrapper(MockContext mock)
    {
        _mock = mock;
    }

    public string GetHeader(string headerName)
    {
        if (_rawReq != null)
            return _rawReq.Headers.Get(headerName);
        if (_mock != null)
            return _mock.RequestHeaders.GetValueOrDefault(headerName);
        return null;
    }

    public string GetPathParam(string key)
    {
        if (_pathParams != null && _pathParams.TryGetValue(key, out var value))
            return value;
        return string.Empty;
    }

    internal void SetPathParams(Dictionary<string, string> pathParams)
    {
        _pathParams = pathParams;
    }
}
