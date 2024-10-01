namespace Backynet;

public interface IWatchdogService
{
    Task Start(CancellationToken cancellationToken);

    IWatchdogCancellationScope Create(Guid jobId);
    void Return(IWatchdogCancellationScope watchdogCancellationScope);
}

public interface IWatchdogCancellationScope : IAsyncDisposable
{
    Guid JobId { get; }
    CancellationToken CancellationToken { get; }
}