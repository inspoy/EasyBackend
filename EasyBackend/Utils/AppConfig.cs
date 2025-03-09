using JetBrains.Annotations;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace EasyBackend.Utils;

public class AppConfig
{
    public dynamic RawYaml;
    public string Host;
    public int Port;
    public AppConfigLogging Logging;

    [CanBeNull]
    public static AppConfig ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        try
        {
            var yml = File.ReadAllText(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var ret = deserializer.Deserialize<AppConfig>(yml);
            ret.RawYaml = deserializer.Deserialize<dynamic>(yml);
            return ret;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Failed to read config file: " + e.Message);
            return null;
        }
    }
}

public class AppConfigLogging
{
    public bool ConsoleEnabled;
    public bool ConsoleColor;
    public string LogFileFolder;
}
