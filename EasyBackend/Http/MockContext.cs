namespace EasyBackend.Http;

public class MockContext
{
    public string Method { get; init; }
    public string RemoteAddress { get; init; }
    public Uri Url { get; init; }
    public string Body { get; init; }
    public Dictionary<string, string> RequestHeaders { get; init; }
    public Dictionary<string, string> ResponseHeaders { get; init; }
}
