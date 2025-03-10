namespace EasyBackend.Http;

public partial class RequestWrapper
{
    public const string ReqIdSavePath = "./reqId.txt";
    public const ulong DefaultStartReqId = 10000;
    private static ulong _reqId = DefaultStartReqId;

    public static void ResetReqId(ulong reqId) => _reqId = reqId;
    public static void SaveReqId() => File.WriteAllText(ReqIdSavePath, _reqId.ToString());
}
