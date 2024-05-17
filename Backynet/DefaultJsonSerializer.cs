using System.Buffers;
using Backynet.Abstraction;
using Newtonsoft.Json;

namespace Backynet;

internal sealed class DefaultJsonSerializer : ISerializer
{
    public static readonly ISerializer Instance = new DefaultJsonSerializer();
    
    private static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    public string Serialize<T>(T instance)
    {
        return JsonConvert.SerializeObject(instance, DefaultJsonSerializerSettings);
    }

    public T Deserialize<T>(string payload)
    {
        var obj = JsonConvert.DeserializeObject<T>(payload, DefaultJsonSerializerSettings);
        return obj == null ? throw new InvalidOperationException() : obj;
    }
}

internal interface IFastSerializer
{
    Stream Serialize<T>(T instance);
    T Deserialize<T>(Stream stream);
}
