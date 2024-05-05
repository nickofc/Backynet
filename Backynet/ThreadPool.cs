namespace Backynet;

public sealed class ThreadPool : IThreadPool
{
    private readonly SemaphoreSlim _semaphoreSlim;

    public ThreadPool(IThreadPoolOptions threadPoolOptions)
    {
        _semaphoreSlim = new SemaphoreSlim(threadPoolOptions.MaxThreads);
    }

    public void Post(Func<Task> func)
    {
        System.Threading.ThreadPool.QueueUserWorkItem(state => { (state as Func<Task>)(); }, func);
    }
}