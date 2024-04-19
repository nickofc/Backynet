namespace Backynet.Core;

public sealed class DefaultThreadPool : IThreadPool, IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim;

    public DefaultThreadPool(int threadCount)
    {
        _semaphoreSlim = new SemaphoreSlim(threadCount);
    }

    public async Task<IDisposable> Acquire(CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        return new ThreadPoolScope(_semaphoreSlim);
    }

    public Task PostAsync(Func<Task> func)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
        {
            var d = state as Func<Task>;
            _ = d();
        }), func);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _semaphoreSlim.Dispose();
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