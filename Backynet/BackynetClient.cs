using System.Linq.Expressions;
using Backynet.Abstraction;

namespace Backynet;

internal sealed class BackynetClient : IBackynetClient
{
    private readonly IJobRepository _jobRepository;

    public BackynetClient(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public Task<string> EnqueueAsync(Expression<Func<Task>> expression, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return EnqueueCoreAsync(expression, null, cancellationToken);
    }

    public Task<string> EnqueueAsync(Expression<Action> expression, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return EnqueueCoreAsync(expression, null, cancellationToken);
    }

    public Task<string> EnqueueAsync(Expression<Func<Task>> expression, string groupName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(groupName);

        return EnqueueCoreAsync(expression, groupName, cancellationToken);
    }

    public Task<string> EnqueueAsync(Expression<Action> expression, string groupName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(groupName);

        return EnqueueCoreAsync(expression, groupName, cancellationToken);
    }

    private async Task<string> EnqueueCoreAsync(Expression expression, string? groupName = null, CancellationToken cancellationToken = default)
    {
        var jobDescriptor = JobDescriptorFactory.Create(expression);
        var job = JobFactory.Create(jobDescriptor);
        job.GroupName = groupName;
        await _jobRepository.Add(job, cancellationToken);
        return job.Id.ToString();
    }
}