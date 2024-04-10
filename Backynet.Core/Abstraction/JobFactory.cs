namespace Backynet.Core.Abstraction;

public static class JobFactory
{
    public static Job Create(IJobDescriptor jobDescriptor)
    {
        ArgumentNullException.ThrowIfNull(jobDescriptor);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            JobState = JobState.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            Descriptor = jobDescriptor
        };

        return job;
    }
}