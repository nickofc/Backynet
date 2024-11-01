namespace Backynet;

public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync();
}
