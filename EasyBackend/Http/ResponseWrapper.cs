using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyBackend.Http;

public class ResponseWrapper
{
    private readonly HttpListenerResponse _rawRes;
    private readonly MockContext _mock;

    public ulong ReqId { get; }
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
    public ResponseErrCode ErrCode { get; set; } = ResponseErrCode.Unknown;
    public JObject Result { get; } = new();
    public string BriefInfo => $"{ReqId}-[{(int)StatusCode} {StatusCode}]({ErrCode}){ToJson()}";

    public ResponseWrapper(ulong reqId, HttpListenerResponse rawRes)
    {
        _rawRes = rawRes;
        ReqId = reqId;
    }

    public ResponseWrapper(ulong reqId, MockContext mock)
    {
        _mock = mock;
        ReqId = reqId;
    }

    public void InitSimple(ResponseErrCode errCode, string result = null)
    {
        switch (errCode)
        {
            case ResponseErrCode.Success:
                StatusCode = HttpStatusCode.OK;
                break;
            case ResponseErrCode.InvalidRequest:
                StatusCode = HttpStatusCode.BadRequest;
                break;
            case ResponseErrCode.InvalidToken:
                StatusCode = HttpStatusCode.Unauthorized;
                break;
            case ResponseErrCode.NotImplement:
                StatusCode = HttpStatusCode.NotImplemented;
                break;
            case ResponseErrCode.NotFound:
                StatusCode = HttpStatusCode.NotFound;
                break;
            case ResponseErrCode.TooManyRequests:
                StatusCode = HttpStatusCode.TooManyRequests;
                break;
            default:
                StatusCode = HttpStatusCode.InternalServerError;
                break;
        }

        ErrCode = errCode;
        if (!string.IsNullOrEmpty(result))
        {
            Result["Message"] = result;
        }
    }

    public string ToJson()
    {
        if (!Result.ContainsKey("Code"))
            Result.AddFirst(new JProperty("Code", (int)ErrCode));
        if (!Result.ContainsKey("Request"))
            Result.AddFirst(new JProperty("Request", RequestWrapper.ReqIdHash(ReqId)));
        return JsonConvert.SerializeObject(Result);
    }

    public void SetHeader(string key, string value)
    {
        _rawRes?.AddHeader(key, value);
        if (_mock != null) _mock.ResponseHeaders[key] = value;
    }
}
