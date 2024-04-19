namespace Backynet.Core;

public interface IThreadPool
{
    Task<IDisposable> Acquire(CancellationToken cancellationToken);
    Task PostAsync(Func<Task> func);
}