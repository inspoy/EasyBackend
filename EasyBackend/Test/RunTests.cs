using EasyBackend.Http;
using EasyBackend.Routing;
using EasyBackend.Utils;
using Newtonsoft.Json;

namespace EasyBackend.Test;

public static class RunTests
{
    public static void Run()
    {
        TestPatternMatching();
    }

    private static void TestPatternMatching()
    {
        void TestOne(string pattern, params string[] paths)
        {
            var handler = new RequestHandler("GET", pattern, null, 0);
            foreach (var path in paths)
            {
                var ok = handler.TestPath(path, out var result);
                Console.WriteLine("{0} - {1}: {2}", pattern, path, ok);
                Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.None));
            }
        }

        TestOne("/user/{id}", "/user/123", "/user/abc", "/account/123");
        TestOne("/user/*", "/user/123", "/user/123/abc", "/user/");
        TestOne("/user/{id}/*", "/user/123", "/user/123/abc", "/account/123");
    }
}
