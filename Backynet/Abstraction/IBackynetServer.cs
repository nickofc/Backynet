namespace Backynet.Abstraction;

public interface IBackynetServer
{
    Task Start(CancellationToken cancellationToken);
}