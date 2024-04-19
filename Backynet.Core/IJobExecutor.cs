using Backynet.Core.Abstraction;

namespace Backynet.Core;

public interface IJobExecutor
{
    Task Execute(Job job, CancellationToken cancellationToken = default);
}