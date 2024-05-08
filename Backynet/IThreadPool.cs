namespace Backynet;

public interface IThreadPool
{
    int AvailableThreadCount { get; }
    Task Post(Func<Task> methodCall, CancellationToken cancellationToken = default);
    Task WaitForAvailableThread(CancellationToken cancellationToken = default);
}