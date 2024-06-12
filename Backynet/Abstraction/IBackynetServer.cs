namespace Backynet.Abstraction;

public interface IBackynetServer
{
    bool IsRunning { get; }

    Task Start(CancellationToken cancellationToken);
    Task WaitForShutdown(CancellationToken cancellationToken = default);
}