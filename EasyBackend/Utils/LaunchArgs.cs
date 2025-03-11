using System.Text;
using JetBrains.Annotations;

namespace EasyBackend.Utils;

internal class LaunchArgItem
{
    public string Name { get; }
    public char Abbr { get; }
    public string Description { get; }
    public string Value { get; set; }

    public LaunchArgItem(string name, string desc) : this(name, '\0', desc)
    {
    }

    public LaunchArgItem(string name, char abbr = '\0', string desc = "")
    {
        Name = name;
        Abbr = abbr;
        Description = desc;
        Value = null;
    }
}

internal class LaunchArgs(string[] rawArgs)
{
    private List<LaunchArgItem> _argBook;
    private readonly Dictionary<string, string> _extraArgs = new();

    /// <summary>
    /// 检查并解析启动参数
    /// </summary>
    public void Check(List<LaunchArgItem> argBook)
    {
        _argBook = argBook;

        var dictArgBook = argBook.ToDictionary(el => el.Name);
        var idx = 0;
        while (idx < rawArgs.Length)
        {
            var arg = rawArgs[idx];
            if (arg.StartsWith("--") && arg.Length > 2)
            {
                var key = arg.Substring(2);
                var value = idx + 1 < rawArgs.Length ? rawArgs[idx + 1] : null;
                if (!string.IsNullOrEmpty(value) && value[0] == '-')
                {
                    value = null;
                }

                if (dictArgBook.TryGetValue(key, out var argItem))
                {
                    argItem.Value = value ?? "True";
                }
                else
                {
                    _extraArgs[key] = value ?? "True";
                }

                idx += 2;
            }
            else if (arg.StartsWith("-") && arg.Length > 1)
            {
                var abbr = arg[1];
                var value = idx + 1 < rawArgs.Length ? rawArgs[idx + 1] : null;
                if (!string.IsNullOrEmpty(value) && value[0] == '-')
                {
                    value = null;
                }

                var argItem = _argBook.FirstOrDefault(el => el.Abbr == abbr);
                if (argItem != null)
                {
                    argItem.Value = value ?? "True";
                }
                else
                {
                    _extraArgs[abbr.ToString()] = value ?? "True";
                }

                idx += 2;
            }
            else
            {
                // Skip
                idx++;
            }
        }
    }

    public string HelpString()
    {
        var sb = new StringBuilder();
        sb.Append("Usage:\n");
        foreach (var launchArgItem in _argBook)
        {
            sb.Append($"   --{launchArgItem.Name}");
            if (launchArgItem.Abbr != '\0')
            {
                sb.Append($", -{launchArgItem.Abbr}");
            }

            sb.Append($": {launchArgItem.Description}\n");
        }

        return sb.ToString();
    }

    public string Dump(bool includeUndefined = false, string format = "text")
    {
        var sb = new StringBuilder();
        sb.Append("EasyBackend will start with arguments:\n");
        foreach (var item in _argBook)
        {
            if (string.IsNullOrEmpty(item.Value) && !includeUndefined) continue;
            sb.Append($"  {item.Name}: {item.Value}\n");
        }

        foreach (var (key, value) in _extraArgs)
        {
            sb.Append($"  {key}: {value}\n");
        }

        return sb.ToString();
    }

    [CanBeNull]
    public string Get(string argName)
    {
        if (_argBook == null) return null;
        foreach (var item in _argBook)
        {
            if (item.Name == argName)
            {
                return item.Value;
            }
        }

        return null;
    }
}
