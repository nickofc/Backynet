namespace Backynet.Abstraction;

public interface IBackynetServer : IDisposable, IAsyncDisposable
{
    bool IsRunning { get; }

    Task Start(CancellationToken cancellationToken);
    Task Shutdown(CancellationToken cancellationToken);

    Task WaitForShutdown(CancellationToken cancellationToken = default);
}