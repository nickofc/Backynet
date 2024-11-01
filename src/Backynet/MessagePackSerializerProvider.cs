using Backynet.Abstraction;
using MessagePack;
using MessagePack.Resolvers;

namespace Backynet;

internal sealed class MessagePackSerializerProvider : ISerializer
{
    private static readonly MessagePackSerializerOptions Options = new(new TypelessContractlessStandardResolver());
    
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