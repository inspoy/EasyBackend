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
}
