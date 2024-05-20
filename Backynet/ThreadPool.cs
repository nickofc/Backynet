namespace Backynet;

public sealed class ThreadPool : IThreadPool, IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly EventWaitHandle _taskCompletedEvent;
    private readonly IThreadPoolOptions _options;

    public ThreadPool(IThreadPoolOptions threadPoolOptions)
    {
        _options = threadPoolOptions;
        _taskCompletedEvent = new AutoResetEvent(false);
        _semaphoreSlim = new SemaphoreSlim(threadPoolOptions.MaxThreads);
    }

    public int MaxThreadCount => _options.MaxThreads;
    public int AvailableThreadCount => _semaphoreSlim.CurrentCount;

    public async Task Post(Func<Task> methodCall, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                await methodCall();
            }
            finally
            {
                _semaphoreSlim.Release();
                _taskCompletedEvent.Set();
            }
        }, CancellationToken.None);
    }

    public Task WaitForAvailableThread(CancellationToken cancellationToken)
    {
        if (AvailableThreadCount > 0)
        {
            return Task.CompletedTask;
        }
        
        _taskCompletedEvent.WaitOne();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _taskCompletedEvent.Dispose();
        _semaphoreSlim.Dispose();
    }
}