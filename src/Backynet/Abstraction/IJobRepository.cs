namespace Backynet.Abstraction;

public interface IJobRepository
{
    Task<Job?> Get(Guid jobId, CancellationToken cancellationToken = default);
    Task Add(Job job, CancellationToken cancellationToken = default);
    Task<bool> Update(Guid jobId, Job job, CancellationToken cancellationToken = default);
}