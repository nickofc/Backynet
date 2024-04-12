namespace Backynet.Core.Abstraction;

public interface IBackynetWorker
{
    Task Start(CancellationToken cancellationToken);
}