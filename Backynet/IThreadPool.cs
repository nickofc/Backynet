namespace Backynet;

public interface IThreadPool
{
    Task<IDisposable> Acquire(CancellationToken cancellationToken = default);
    Task PostAsync(Func<Task> func);
}