using System.Net;

namespace EasyBackend.Http;

public partial class RequestWrapper(HttpListenerRequest rawReq)
{
    public HttpListenerRequest RawReq => rawReq;
    public ulong ReqId { get; } = _reqId++;

    public string BriefInfo =>
        $"{ReqId}-[{rawReq.HttpMethod}] {rawReq.Url?.LocalPath} from {ClientIp}, q={Query.Count}, b={Body.Length}";

    public string ClientIp =>
        _clientIp ??= rawReq.Headers.Get("X-Real-Ip") ??
                      rawReq.Headers.Get("Remote_Ip") ??
                      rawReq.RemoteEndPoint?.Address.ToString();

    public Dictionary<string, string> Query
    {
        get
        {
            if (_queryDict != null) return _queryDict;

            var query = rawReq.Url?.Query;
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
            _body = new StreamReader(rawReq.InputStream).ReadToEnd();
            return _body;
        }
    }

    private string _body;
    private string _clientIp;
    private Dictionary<string, string> _queryDict;
}
