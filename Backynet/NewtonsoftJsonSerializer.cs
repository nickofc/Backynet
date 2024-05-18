using Backynet.Abstraction;
using MessagePack;
using MessagePack.Resolvers;

namespace Backynet;

internal sealed class NewtonsoftJsonSerializer : ISerializer
{
    public ReadOnlyMemory<byte> Serialize<T>(T instance)
    {
        throw new NotImplementedException();
    }

    public T Deserialize<T>(ReadOnlyMemory<byte> data)
    {
        throw new NotImplementedException();
    }
}

internal sealed class MessagePackSerializerProvider : ISerializer
{
    private static readonly MessagePackSerializerOptions Options = new MessagePackSerializerOptions(new TypelessContractlessStandardResolver());
    
    public ReadOnlyMemory<byte> Serialize<T>(T instance)
    {
        var data = MessagePackSerializer.Serialize(instance, Options);
        return new ReadOnlyMemory<byte>(data);
    }

    public T Deserialize<T>(ReadOnlyMemory<byte> data)
    {
        return MessagePackSerializer.Deserialize<T>(data, Options);
    }
}