namespace Backynet;

public interface IThreadPool
{
    int AvailableThreadCount { get; }
    Task Post(Func<Task> methodCall);
    Task WaitForAvailableThread(CancellationToken cancellationToken);
}