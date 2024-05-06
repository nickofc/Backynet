namespace Backynet;

public sealed class ThreadPool : IThreadPool
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private int _availableThreadCount;

    public ThreadPool(IThreadPoolOptions threadPoolOptions)
    {
        _semaphoreSlim = new SemaphoreSlim(threadPoolOptions.MaxThreads);
        _availableThreadCount = threadPoolOptions.MaxThreads;
    }

    public int AvailableThreadCount => _availableThreadCount;

    public Task Post(Func<Task> methodCall)
    {
        _ = Task.Run(async () =>
        {
            await _semaphoreSlim.WaitAsync();
            Interlocked.Decrement(ref _availableThreadCount);

            try
            {
                await methodCall();
            }
            finally
            {
                _semaphoreSlim.Release();
                Interlocked.Increment(ref _availableThreadCount);
            }
        });

        return Task.CompletedTask;
    }

    public Task WaitForAvailableThread(CancellationToken cancellationToken)
    {
        return _semaphoreSlim.WaitAsync(cancellationToken);
    }
}