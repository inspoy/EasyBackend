using System.Net;
using Newtonsoft.Json;

namespace EasyBackend.Http;

public class ResponseWrapper(ulong reqId, HttpListenerResponse rawRes)
{
    [JsonIgnore] public ulong ReqId { get; } = reqId;
    public string ReqHash { get; } = RequestWrapper.ReqIdHash(reqId);
    public HttpStatusCode StatusCode { get; set; }
    public ResponseErrCode ErrCode { get; set; }
    public string Result { get; set; }
    [JsonIgnore] public string BriefInfo => $"{ReqId}-[{(int)StatusCode} {StatusCode}]({ErrCode}){Result}";

    public void InitSimple(ResponseErrCode errCode, string result)
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
        Result = result;
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public void SetHeader(string key, string value)
    {
        rawRes.AddHeader(key, value);
    }
}
