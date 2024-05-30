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

    public async Task<string> EnqueueAsync(
        Expression expression,
        string? groupName = null,
        DateTimeOffset? when = null,
        string? cron = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var jobDescriptor = JobDescriptorFactory.Create(expression);
        var job = JobFactory.Create(jobDescriptor);

        if (groupName != null)
        {
            job.GroupName = groupName;
        }

        if (when != null)
        {
            job.NextOccurrenceAt = when;
        }

        if (cron != null)
        {
            job.Cron = cron;
        }

        await _jobRepository.Add(job, cancellationToken);

        return job.Id.ToString();
    }
}