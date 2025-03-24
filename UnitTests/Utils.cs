using EasyBackend.Http;

namespace UnitTests;

public static class Utils
{
    /// <summary>
    /// 获取指定代码块的标准输出
    /// </summary>
    /// <param name="code"></param>
    public static string GetOutput(TestDelegate code)
    {
        var originalOut = Console.Out;
        string result;
        try
        {
            using var writer = new StringWriter();
            Console.SetOut(writer);
            code();
            result = writer.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return result;
    }

    public static string CreateTempConfig()
    {
        var testContent =
            """
            host: http://localhost
            port: 8080
            logging:
              consoleEnabled: true
              consoleColor: true
              logFileFolder: ./logs
            reload:
              enabled: true
              path: /reload_config
              token: 123
            otherField:
              field1: value1
              field2: value2
            """;
        var tempDir = Directory.CreateTempSubdirectory("EasyBackend_");
        Console.WriteLine(tempDir);
        var cfgPath = Path.Combine(tempDir.FullName, "appConf.yml");
        File.WriteAllText(cfgPath, testContent);
        return cfgPath;
    }
    
    public static MockContext CreateMockContext(string method, string url, string body = null)
    {
        return new MockContext
        {
            Method = method,
            RemoteAddress = "0.0.0.0",
            Url = new Uri("http://0.0.0.0" + url),
            Body = body,
            RequestHeaders = new Dictionary<string, string>(),
            ResponseHeaders = new Dictionary<string, string>()
        };
    }
}
