namespace Backynet.Abstraction;

public interface ISerializer
{
    ReadOnlyMemory<byte> Serialize<T>(T instance);
    T Deserialize<T>(ReadOnlyMemory<byte> data);
}