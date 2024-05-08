namespace Backynet;

public sealed class ThreadPool : IThreadPool, IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private int _availableThreadCount;

    public ThreadPool(IThreadPoolOptions threadPoolOptions)
    {
        _semaphoreSlim = new SemaphoreSlim(threadPoolOptions.MaxThreads);
        _availableThreadCount = threadPoolOptions.MaxThreads;
    }

    public int AvailableThreadCount => _availableThreadCount;

    public async Task Post(Func<Task> methodCall, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        Interlocked.Decrement(ref _availableThreadCount);

        _ = Task.Run(async () =>
        {
            try
            {
                await methodCall();
            }
            finally
            {
                _semaphoreSlim.Release();
                Interlocked.Increment(ref _availableThreadCount);
            }
        }, CancellationToken.None);
    }

    public Task WaitForAvailableThread(CancellationToken cancellationToken)
    {
        return _semaphoreSlim.WaitAsync(cancellationToken);
    }

    public void Dispose()
    {
        _semaphoreSlim.Dispose();
    }
}