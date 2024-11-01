namespace Backynet;

public interface IWatchdogService
{
    Task Start(CancellationToken cancellationToken);

    CancellationToken Rent(Guid jobId);
    void Return(Guid jobId);
}