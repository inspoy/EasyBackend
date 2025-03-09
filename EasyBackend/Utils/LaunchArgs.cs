using System.Text;
using JetBrains.Annotations;

namespace EasyBackend.Utils;

internal class LaunchArgs(string[] rawArgs)
{
    private readonly Dictionary<string, string> _args = new();

    /// <summary>
    /// 检查并解析启动参数
    /// </summary>
    public void Check(Dictionary<char, string> abbrMapping)
    {
        if (rawArgs.Length == 0) return;

        var idx = 0;
        while (idx < rawArgs.Length)
        {
            var arg = rawArgs[idx];
            if (arg.StartsWith("--"))
            {
                var key = arg.Substring(2);
                var value = idx + 1 < rawArgs.Length ? rawArgs[idx + 1] : null;
                _args[key] = value;
                idx += 2;
            }
            else if (arg.StartsWith("-"))
            {
                var key = arg.Substring(1);
                var value = idx + 1 < rawArgs.Length ? rawArgs[idx + 1] : null;
                if (abbrMapping.ContainsKey(key[0]))
                {
                    _args[abbrMapping[key[0]]] = value;
                }

                idx += 2;
            }
            else
            {
                idx++;
            }
        }
    }

    public string Dump(string format = "text")
    {
        var sb = new StringBuilder();
        sb.Append("EasyBackend will start with arguments:\n");
        foreach (var (key, value) in _args)
        {
            // if (key.Length == 1) continue;
            sb.Append($"   --{key}: {value}\n");
        }

        return sb.ToString();
    }

    [CanBeNull]
    public string Get(string argName)
    {
        if (_args.TryGetValue(argName, out var arg))
        {
            if (arg == null) arg = "True";
            return arg;
        }

        return null;
    }
}
