namespace Backynet.Core;

public class DefaultThreadPool : IThreadPool
{
    private readonly SemaphoreSlim _semaphoreSlim = new(10);
    
    public async Task<IDisposable> Acquire(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        return new ThreadPoolScope(_semaphoreSlim);
    }

    public Task PostAsync(Func<Task> func)
    {
        throw new NotImplementedException();
    }
}

internal readonly struct ThreadPoolScope : IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim;

    public ThreadPoolScope(SemaphoreSlim semaphoreSlim)
    {
        _semaphoreSlim = semaphoreSlim;
    }

    public void Dispose()
    {
        _semaphoreSlim.Release();
    }
}