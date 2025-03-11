using System.IO.Hashing;
using System.Text;

namespace EasyBackend.Http;

public partial class RequestWrapper
{
    private const string ReqIdSavePath = "./reqId.txt";
    private const ulong DefaultStartReqId = 10000;
    private static readonly byte[] HashBuffer = [0, 0, 0, 0, 0, 0, 0, 0, 0xEA, 0x51, 0xBA, 0xC4, 0xE1, 0xDD];
    private static readonly StringBuilder HexBuffer = new(19);
    private static ulong _reqId = DefaultStartReqId;

    public static ulong ResetReqId()
    {
        try
        {
            var lastReqId = File.ReadAllText(ReqIdSavePath);
            if (ulong.TryParse(lastReqId, out var reqId))
            {
                _reqId = reqId;
            }
        }
        catch (FileNotFoundException)
        {
            File.WriteAllText(ReqIdSavePath, DefaultStartReqId.ToString());
            _reqId = DefaultStartReqId;
        }
        catch (Exception)
        {
            _reqId = DefaultStartReqId;
        }

        return _reqId;
    }

    public static void ResetReqId(ulong reqId) => _reqId = reqId;

    public static void SaveReqId()
    {
        lock (HashBuffer)
        {
            File.WriteAllText(ReqIdSavePath, _reqId.ToString());
        }
    }

    public static string ReqIdHash(ulong reqId)
    {
        lock(HashBuffer)
        {
            HashBuffer[0] = (byte)(reqId >> 56);
            HashBuffer[1] = (byte)(reqId >> 48);
            HashBuffer[2] = (byte)(reqId >> 40);
            HashBuffer[3] = (byte)(reqId >> 32);
            HashBuffer[4] = (byte)(reqId >> 24);
            HashBuffer[5] = (byte)(reqId >> 16);
            HashBuffer[6] = (byte)(reqId >> 8);
            HashBuffer[7] = (byte)reqId;
            var hash = XxHash64.HashToUInt64(HashBuffer);
            HexBuffer.Clear();
            HexBuffer.Append((hash >> 48).ToString("x4"));
            HexBuffer.Append('-');
            HexBuffer.Append((hash >> 16 & 0xFFFFFFFF).ToString("x8"));
            HexBuffer.Append('-');
            HexBuffer.Append((hash & 0xFFFF).ToString("x4"));
            return HexBuffer.ToString();
        }
    }
}
