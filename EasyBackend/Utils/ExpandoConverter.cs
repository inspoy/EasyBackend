using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace EasyBackend.Utils;

public class ExpandoConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(ExpandoObject) || type == typeof(object);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        // 处理映射开始事件
        if (parser.Current is MappingStart)
        {
            var result = new ExpandoObject();
            var resultDict = (IDictionary<string, object>)result;

            parser.MoveNext();

            while (parser.Current is not MappingEnd)
            {
                // 获取键（必须是标量）
                if (parser.Current is Scalar scalar)
                {
                    var key = scalar.Value;
                    parser.MoveNext();

                    // 解析值
                    object value = ReadValue(parser, rootDeserializer);
                    resultDict[key] = value;
                }
                else
                {
                    throw new YamlException("Expected scalar key");
                }
            }

            parser.MoveNext(); // 消费 MappingEnd
            return result;
        }

        throw new YamlException("Expected mapping node");
    }

    private object ReadValue(IParser parser, ObjectDeserializer rootDeserializer)
    {
        switch (parser.Current)
        {
            case Scalar scalar:
                parser.MoveNext();
                if (scalar.Value == "~" || scalar.Value == "null" || string.IsNullOrEmpty(scalar.Value))
                    return null;

                // 尝试解析为数字或布尔值
                if (scalar.Style == ScalarStyle.Plain)
                {
                    if (bool.TryParse(scalar.Value, out bool boolResult))
                        return boolResult;

                    if (int.TryParse(scalar.Value, out int intResult))
                        return intResult;

                    if (double.TryParse(scalar.Value, out double doubleResult))
                        return doubleResult;
                }

                return scalar.Value;

            case MappingStart:
                return ReadYaml(parser, typeof(ExpandoObject), rootDeserializer);

            case SequenceStart:
                var list = new List<object>();
                parser.MoveNext();

                while (parser.Current is not SequenceEnd)
                {
                    list.Add(ReadValue(parser, rootDeserializer));
                }

                parser.MoveNext(); // 消费 SequenceEnd
                return list;

            default:
                throw new YamlException($"Unsupported YAML node type: {parser.Current.GetType().Name}");
        }
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        // 如果需要序列化功能，这里实现
        throw new NotImplementedException("Serialization to YAML is not implemented");
    }
}
