using Backynet.Abstraction;

namespace Backynet;

public interface IJobExecutor
{
    Task Execute(Job job, CancellationToken cancellationToken = default);
}