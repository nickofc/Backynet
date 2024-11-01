namespace Backynet;

public class NullTransactionScope : ITransactionScope
{
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task CommitAsync()
    {
        return Task.CompletedTask;
    }
}