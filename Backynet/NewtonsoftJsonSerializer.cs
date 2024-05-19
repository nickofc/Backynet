using Backynet.Abstraction;

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