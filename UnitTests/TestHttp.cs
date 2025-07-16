using EasyBackend.Http;
using EasyBackend.Routing;

namespace UnitTests;

public class TestHttp
{
    [Test]
    public void TestResponseWrapper()
    {
        var ctx = Utils.CreateMockContext("GET", "/");
        var resp = new ResponseWrapper(12345, ctx);
        resp.InitSimple(ResponseErrCode.NotImplement, "Only for testing");
        Assert.That(resp.BriefInfo, Is.EqualTo(
            """
            12345-[501 NotImplemented](NotImplement){"Request":"3bad-d25ccda3-350f","Code":4,"Message":"Only for testing"}
            """));
    }

    [Test]
    public void TestThrottleMiddleware()
    {
        var throttle = new ThrottleMiddleware(2000, 5);
        for (var i = 0; i < 10; ++i)
        {
            var ctx = Utils.CreateMockContext("GET", "/");
            var req = new RequestWrapper(ctx);
            var res = new ResponseWrapper(req.ReqId, ctx);
            var result = throttle.PreExecute(req, res);
            if (i < 5)
            {
                Assert.That(result, Is.True);
            }
            else
            {
                Assert.That(result, Is.False);
                Assert.That(res.ErrCode, Is.EqualTo(ResponseErrCode.TooManyRequests));
            }
        }
    }

    [Test]
    public static void TestPathParse()
    {
        var ctx = Utils.CreateMockContext("GET", "/hello?a=1&b=2");
        var req = new RequestWrapper(ctx);
        req.Query.TryGetValue("a", out var a);
        req.Query.TryGetValue("b", out var b);
        Assert.That(a, Is.EqualTo("1"));
        Assert.That(b, Is.EqualTo("2"));
        Assert.That(req.Query, Does.Not.ContainKey("c"));
    }
}
